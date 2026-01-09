using Microsoft.EntityFrameworkCore;
using server.Data;
using System.Diagnostics;

namespace server.Services
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;
        private readonly string _backupDirectory;

        public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Создаем директорию для резервных копий
            _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Connection string not found");
                }

                // Парсим connection string для получения параметров
                var connectionParams = ParseConnectionString(connectionString);
                
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupFileName = $"Nekrasovsky_backup_{timestamp}.sql";
                var backupFilePath = Path.Combine(_backupDirectory, backupFileName);

                // Используем pg_dump для создания резервной копии
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "pg_dump",
                    Arguments = $"-h {connectionParams.Host} -p {connectionParams.Port} -U {connectionParams.Username} -d {connectionParams.Database} -F c -f \"{backupFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Устанавливаем переменную окружения для пароля
                processStartInfo.Environment["PGPASSWORD"] = connectionParams.Password;

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    throw new Exception("Failed to start pg_dump process");
                }

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    throw new Exception($"pg_dump failed: {error}");
                }

                _logger.LogInformation("Backup created successfully: {BackupFilePath}", backupFilePath);
                return backupFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                throw;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException($"Backup file not found: {backupFilePath}");
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Connection string not found");
                }

                var connectionParams = ParseConnectionString(connectionString);

                // Используем pg_restore для восстановления
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "pg_restore",
                    Arguments = $"-h {connectionParams.Host} -p {connectionParams.Port} -U {connectionParams.Username} -d {connectionParams.Database} -c \"{backupFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                processStartInfo.Environment["PGPASSWORD"] = connectionParams.Password;

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    throw new Exception("Failed to start pg_restore process");
                }

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogError("pg_restore failed: {Error}", error);
                    return false;
                }

                _logger.LogInformation("Backup restored successfully from: {BackupFilePath}", backupFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                return false;
            }
        }

        public Task<List<string>> GetBackupFilesAsync()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupDirectory, "*.sql")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .ToList();

                return Task.FromResult(backupFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup files");
                return Task.FromResult(new List<string>());
            }
        }

        public Task<bool> DeleteBackupAsync(string backupFilePath)
        {
            try
            {
                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                    _logger.LogInformation("Backup file deleted: {BackupFilePath}", backupFilePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backup file");
                return Task.FromResult(false);
            }
        }

        private (string Host, string Port, string Username, string Password, string Database) ParseConnectionString(string connectionString)
        {
            var host = "localhost";
            var port = "5432";
            var username = "postgres";
            var password = "";
            var database = "";

            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();

                    switch (key.ToLower())
                    {
                        case "host":
                            host = value;
                            break;
                        case "port":
                            port = value;
                            break;
                        case "username":
                        case "user id":
                            username = value;
                            break;
                        case "password":
                            password = value;
                            break;
                        case "database":
                        case "initial catalog":
                            database = value;
                            break;
                    }
                }
            }

            return (host, port, username, password, database);
        }
    }
}
