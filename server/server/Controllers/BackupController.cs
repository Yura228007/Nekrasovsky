using Microsoft.AspNetCore.Mvc;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(IBackupService backupService, ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        // POST: api/backup/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                _logger.LogInformation("Manual backup requested");
                var backupPath = await _backupService.CreateBackupAsync();
                return Ok(new { message = "Backup created successfully", path = backupPath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual backup");
                return StatusCode(500, new { message = $"Error creating backup: {ex.Message}" });
            }
        }

        // GET: api/backup/list
        [HttpGet("list")]
        public async Task<IActionResult> GetBackupList()
        {
            try
            {
                var backupFiles = await _backupService.GetBackupFilesAsync();
                var fileInfo = backupFiles.Select(file => new
                {
                    path = file,
                    fileName = Path.GetFileName(file),
                    created = File.GetCreationTime(file),
                    size = new FileInfo(file).Length
                }).ToList();

                return Ok(fileInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup list");
                return StatusCode(500, new { message = $"Error getting backup list: {ex.Message}" });
            }
        }

        // POST: api/backup/restore
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreBackup([FromBody] RestoreBackupRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.BackupFilePath))
                {
                    return BadRequest(new { message = "Backup file path is required" });
                }

                _logger.LogWarning("Manual backup restore requested: {BackupFilePath}", request.BackupFilePath);
                var success = await _backupService.RestoreBackupAsync(request.BackupFilePath);

                if (success)
                {
                    return Ok(new { message = "Backup restored successfully" });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to restore backup" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                return StatusCode(500, new { message = $"Error restoring backup: {ex.Message}" });
            }
        }

        // DELETE: api/backup/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBackup([FromBody] DeleteBackupRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.BackupFilePath))
                {
                    return BadRequest(new { message = "Backup file path is required" });
                }

                var success = await _backupService.DeleteBackupAsync(request.BackupFilePath);

                if (success)
                {
                    return Ok(new { message = "Backup deleted successfully" });
                }
                else
                {
                    return NotFound(new { message = "Backup file not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backup");
                return StatusCode(500, new { message = $"Error deleting backup: {ex.Message}" });
            }
        }
    }

    public class RestoreBackupRequest
    {
        public string BackupFilePath { get; set; } = string.Empty;
    }

    public class DeleteBackupRequest
    {
        public string BackupFilePath { get; set; } = string.Empty;
    }
}
