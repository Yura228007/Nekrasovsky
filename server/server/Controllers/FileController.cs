using Microsoft.AspNetCore.Mvc;
using server.Data;
using server.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public FileController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("???? ?? ?????? ??? ????.");

        // ????? ??? ????????
        var uploadFolder = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadFolder);

        // ????????? ??????????? ?????
        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        // ?????????? ????? ?? ????
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // ?????????? ?????????? ? ??
        var uploadedFile = new UploadedFile
        {
            FileName = file.FileName,
            FilePath = filePath,
            FileSize = file.Length
        };

        _context.UploadedFiles.Add(uploadedFile);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "???? ??????? ????????",
            FileId = uploadedFile.Id,
            FileName = uploadedFile.FileName
        });
    }
}