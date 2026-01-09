using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Extensions;
using server.Middleware;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Kestrel with SSL certificate
builder.WebHost.ConfigureKestrel(options =>
{
    var certificatePath = builder.Configuration["Kestrel:Certificates:Default:Path"];
    var certificatePassword = builder.Configuration["Kestrel:Certificates:Default:Password"];

    if (!string.IsNullOrEmpty(certificatePath) && File.Exists(certificatePath))
    {
        try
        {
            // Используем кастомный сертификат из файла
            var certificate = string.IsNullOrEmpty(certificatePassword)
                ? new X509Certificate2(certificatePath)
                : new X509Certificate2(certificatePath, certificatePassword);
            
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ServerCertificate = certificate;
            });
        }
        catch (Exception ex)
        {
            // Если не удалось загрузить сертификат, используем dev сертификат
            Console.WriteLine($"Предупреждение: Не удалось загрузить SSL сертификат из {certificatePath}: {ex.Message}");
            Console.WriteLine("Используется встроенный dev SSL сертификат");
        }
    }
    // Если путь не указан или файл не существует, используется встроенный dev сертификат
});

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddApplicationServices();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying database migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        // Don't throw - allow the app to start even if migration fails
        // This way you can fix the issue and restart
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<AuditMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();