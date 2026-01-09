namespace server.Services
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();
        Task<bool> RestoreBackupAsync(string backupFilePath);
        Task<List<string>> GetBackupFilesAsync();
        Task<bool> DeleteBackupAsync(string backupFilePath);
    }
}
