using Microsoft.Extensions.Options;

namespace server.Services
{
    public class BackupBackgroundService : BackgroundService
    {
        private readonly IBackupService _backupService;
        private readonly ILogger<BackupBackgroundService> _logger;
        private readonly TimeSpan _backupInterval;

        public BackupBackgroundService(
            IBackupService backupService,
            ILogger<BackupBackgroundService> logger,
            IConfiguration configuration)
        {
            _backupService = backupService;
            _logger = logger;
            
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
                    var backupPath = await _backupService.CreateBackupAsync();
                    _logger.LogInformation("Scheduled backup completed: {BackupPath}", backupPath);

                    // Удаляем старые резервные копии (оставляем последние 10)
                    await CleanupOldBackupsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in backup background service");
                }
            }

            _logger.LogInformation("Backup Background Service stopped");
        }

        private async Task CleanupOldBackupsAsync()
        {
            try
            {
                var backupFiles = await _backupService.GetBackupFilesAsync();
                const int maxBackups = 10;

                if (backupFiles.Count > maxBackups)
                {
                    var filesToDelete = backupFiles.Skip(maxBackups).ToList();
                    foreach (var file in filesToDelete)
                    {
                        await _backupService.DeleteBackupAsync(file);
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
