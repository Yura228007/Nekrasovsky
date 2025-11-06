namespace server.Models;

public class UploadedFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;
}