using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace server.Services
{
    public class BackupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BackupBackgroundService> _logger;
        private readonly TimeSpan _backupInterval;
        private readonly IConfiguration _configuration;

        public BackupBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BackupBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _configuration = configuration;
            
            // Получаем интервал из конфигурации (по умолчанию 24 часа)
            var hours = configuration.GetValue<int>("BackupSettings:IntervalHours", 24);
            _backupInterval = TimeSpan.FromHours(hours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Backup Background Service started. Backup interval: {Interval}", _backupInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_backupInterval, stoppingToken);
                    
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    _logger.LogInformation("Starting scheduled backup...");
                    
                    using var scope = _serviceScopeFactory.CreateScope();
                    var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                    
                    var backupPath = await backupService.CreateBackupAsync();
                    _logger.LogInformation("Scheduled backup completed: {BackupPath}", backupPath);

                    // Удаляем старые резервные копии (оставляем последние 10)
                    await CleanupOldBackupsAsync(backupService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in backup background service");
                }
            }

            _logger.LogInformation("Backup Background Service stopped");
        }

        private async Task CleanupOldBackupsAsync(IBackupService backupService)
        {
            try
            {
                var backupFiles = await backupService.GetBackupFilesAsync();
                const int maxBackups = 10;

                if (backupFiles.Count > maxBackups)
                {
                    var filesToDelete = backupFiles.Skip(maxBackups).ToList();
                    foreach (var file in filesToDelete)
                    {
                        await backupService.DeleteBackupAsync(file);
                        _logger.LogInformation("Deleted old backup: {FilePath}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old backups");
            }
        }
    }
}
