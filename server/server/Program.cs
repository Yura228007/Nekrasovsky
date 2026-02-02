using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Extensions;
using server.Middleware;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
    options.OperationFilter<server.Extensions.SwaggerXUserIdOperationFilter>();
});

// Add SignalR
builder.Services.AddSignalR();

// Configure Kestrel to listen on all interfaces
builder.WebHost.ConfigureKestrel(options =>
{
    // Настройка HTTP - слушаем на всех интерфейсах
    // Используем порт 9000, если 7000 занят
    options.ListenAnyIP(9000, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });

    // Настройка HTTPS - слушаем на всех интерфейсах
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
        
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
                
                listenOptions.UseHttps(certificate);
            }
            catch (Exception ex)
            {
                // Если не удалось загрузить сертификат, используем dev сертификат
                Console.WriteLine($"Предупреждение: Не удалось загрузить SSL сертификат из {certificatePath}: {ex.Message}");
                Console.WriteLine("Используется встроенный dev SSL сертификат");
                listenOptions.UseHttps(); // Dev сертификат
            }
        }
        else
        {
            // Если путь не указан или файл не существует, используется встроенный dev сертификат
            listenOptions.UseHttps(); // Dev сертификат
        }
    });
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
        logger.LogInformation("Checking database migrations...");
        
        // Get pending migrations
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        var appliedMigrations = dbContext.Database.GetAppliedMigrations().ToList();
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count);
            try
            {
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully.");
            }
            catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42P07")
            {
                // Table already exists - mark migration as applied
                logger.LogWarning("Some tables already exist. Marking migration as applied...");
                
                var allMigrations = dbContext.Database.GetMigrations().ToList();
                var initialMigration = allMigrations.FirstOrDefault();
                
                if (initialMigration != null && !appliedMigrations.Contains(initialMigration))
                {
                    try
                    {
                        // Create migrations history table if it doesn't exist
                        dbContext.Database.ExecuteSqlRaw(@"
                            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                                ""MigrationId"" varchar(150) NOT NULL,
                                ""ProductVersion"" varchar(32) NOT NULL,
                                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                            );
                        ");
                        
                        // Mark migration as applied
                        dbContext.Database.ExecuteSql(
                            $@"INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                               VALUES ({initialMigration}, '8.0.10')
                               ON CONFLICT (""MigrationId"") DO NOTHING;");
                        
                        logger.LogInformation("Migration {Migration} marked as applied.", initialMigration);
                    }
                    catch (Exception innerEx)
                    {
                        logger.LogWarning(innerEx, "Could not mark migration as applied, but tables exist. Continuing...");
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("Database is up to date. No migrations to apply.");
        }

        // Ensure ProductOutput.ProductBatchId exists (migration may not have been applied on this DB)
        try
        {
            // Try quoted table name first (EF default), then lowercase (PostgreSQL default)
            try
            {
                dbContext.Database.ExecuteSqlRaw(@"ALTER TABLE ""ProductOutput"" ADD COLUMN IF NOT EXISTS ""ProductBatchId"" integer NULL;");
                dbContext.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ""IX_ProductOutput_ProductBatchId"" ON ""ProductOutput"" (""ProductBatchId"");");
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
            {
                // relation "ProductOutput" does not exist — try lowercase
                dbContext.Database.ExecuteSqlRaw(@"ALTER TABLE productoutput ADD COLUMN IF NOT EXISTS ""ProductBatchId"" integer NULL;");
                dbContext.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS ix_productoutput_productbatchid ON productoutput (""ProductBatchId"");");
            }
            logger.LogInformation("ProductOutput.ProductBatchId column ensured.");
        }
        catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "42701")
        {
            // duplicate_column / index already exists — ignore
        }
        catch (Exception colEx)
        {
            logger.LogWarning(colEx, "Could not ensure ProductOutput.ProductBatchId column.");
        }

        try
        {
            server.Seed.RolePermissionSeeder.SeedRolesAndPermissions(dbContext, logger, AppContext.BaseDirectory);
        }
        catch (Exception seedEx)
        {
            logger.LogError(seedEx, "Failed to seed roles and permissions.");
        }

        try
        {
            if (!dbContext.Users.Any())
            {
                var adminRole = dbContext.Roles.FirstOrDefault(r => r.Code == "Admin" || r.Code == "Owner");
                if (adminRole != null)
                {
                    var passwordService = scope.ServiceProvider.GetRequiredService<server.Services.IPasswordService>();
                    var adminUser = new server.Models.User
                    {
                        Login = "admin",
                        Name = "Администратор",
                        Surname = "Система",
                        Email = "admin@local",
                        Phone = "",
                        EncryptedPassword = passwordService.HashPassword("admin"),
                        RoleId = adminRole.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.Users.Add(adminUser);
                    dbContext.SaveChanges();
                    logger.LogInformation("First user created: login=admin, password=admin (role: {Role})", adminRole.Name);
                }
            }
        }
        catch (Exception userEx)
        {
            logger.LogError(userEx, "Failed to seed first user.");
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

app.UseMiddleware<ShiftAccessMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<server.Hubs.NotificationHub>("/hubs/notifications");

app.Run();