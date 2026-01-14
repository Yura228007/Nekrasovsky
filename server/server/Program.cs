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
        logger.LogInformation("Checking database connection...");
        
        // Check if database exists and can connect
        if (!dbContext.Database.CanConnect())
        {
            logger.LogWarning("Cannot connect to database. Please check connection string.");
        }
        else
        {
            logger.LogInformation("Database connection successful.");
            
            // Get pending migrations
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count);
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation("Applying migration: {Migration}", migration);
                }
                
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date. No migrations to apply.");
            }
        }
    }
    catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P07")
    {
        // Table already exists - this means migration was partially applied
        logger.LogWarning("Some tables already exist. Attempting to mark migrations as applied...");
        try
        {
            // Get all migrations
            var allMigrations = dbContext.Database.GetMigrations().ToList();
            var appliedMigrations = dbContext.Database.GetAppliedMigrations().ToList();
            
            // If migrations history table doesn't exist or migration is not marked as applied
            if (!appliedMigrations.Any() && allMigrations.Any())
            {
                logger.LogInformation("Creating migrations history table and marking migrations as applied...");
                
                // Create migrations history table if it doesn't exist
                try
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                            ""MigrationId"" varchar(150) NOT NULL,
                            ""ProductVersion"" varchar(32) NOT NULL,
                            CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                        );
                    ");
                }
                catch
                {
                    // Table might already exist, ignore
                }
                
                // Mark the initial migration as applied
                var initialMigration = allMigrations.FirstOrDefault();
                if (initialMigration != null)
                {
                    try
                    {
                        dbContext.Database.ExecuteSqlRaw($@"
                            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                            VALUES ('{initialMigration}', '8.0.10')
                            ON CONFLICT (""MigrationId"") DO NOTHING;
                        ");
                        logger.LogInformation("Migration {Migration} marked as applied.", initialMigration);
                    }
                    catch (Exception insertEx)
                    {
                        logger.LogWarning(insertEx, "Could not mark migration as applied. This is usually safe to ignore if tables already exist.");
                    }
                }
            }
            
            logger.LogInformation("Database migration issue resolved. Server will continue.");
        }
        catch (Exception innerEx)
        {
            logger.LogError(innerEx, "Failed to resolve migration issue. Please run 'dotnet ef database update' manually.");
        }
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