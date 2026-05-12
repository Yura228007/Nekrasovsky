using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using server.Data;
using server.Models;
using server.Services;

namespace ControllersTests;

internal static class TestDbContextFactory
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static UserPermissionsService CreateUserPermissionsService(AppDbContext context) =>
        new(context, NullLogger<UserPermissionsService>.Instance);

    public static RecipeService CreateRecipeService(AppDbContext context) =>
        new(context, NullLogger<RecipeService>.Instance);

    public static User SeedUser(AppDbContext db, string login = "user1")
    {
        var user = new User
        {
            Login = login,
            EncryptedPassword = "x",
            Name = "N",
            Surname = "S",
            Email = $"{login}@test.local",
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public static Permission SeedPermission(AppDbContext db, string code = "TEST_PERM")
    {
        var permission = new Permission { Code = code, Name = code, Description = "d" };
        db.Permissions.Add(permission);
        db.SaveChanges();
        return permission;
    }

    public static (Product product, Material material) SeedProductAndMaterial(AppDbContext db)
    {
        var product = new Product { Name = "P", MeasuringUnit = "шт", IsActive = true };
        var material = new Material { Name = "M", MeasuringUnit = "кг", IsActive = true };
        db.Products.Add(product);
        db.Materials.Add(material);
        db.SaveChanges();
        return (product, material);
    }
}
