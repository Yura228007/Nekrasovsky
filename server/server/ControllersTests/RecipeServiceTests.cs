using server.Models;

namespace ControllersTests;

public class RecipeServiceTests
{
    [Fact]
    public async Task CreateRecipeAsync_throws_when_product_missing()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var (_, material) = TestDbContextFactory.SeedProductAndMaterial(db);
        var sut = TestDbContextFactory.CreateRecipeService(db);
        var recipe = new Recipe { ProductId = 999, MaterialId = material.Id, Quantity = 1 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.CreateRecipeAsync(recipe));
    }

    [Fact]
    public async Task CreateRecipeAsync_throws_when_material_missing()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var (product, _) = TestDbContextFactory.SeedProductAndMaterial(db);
        var sut = TestDbContextFactory.CreateRecipeService(db);
        var recipe = new Recipe { ProductId = product.Id, MaterialId = 999, Quantity = 1 };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.CreateRecipeAsync(recipe));
    }

    [Fact]
    public async Task CreateRecipeAsync_persists_and_GetRecipe_returns_it()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var (product, material) = TestDbContextFactory.SeedProductAndMaterial(db);
        var sut = TestDbContextFactory.CreateRecipeService(db);
        var recipe = new Recipe
        {
            ProductId = product.Id,
            MaterialId = material.Id,
            Quantity = 2.5,
            MeasuringType = "кг"
        };

        var created = await sut.CreateRecipeAsync(recipe);
        var loaded = await sut.GetRecipeAsync(product.Id, material.Id);

        Assert.NotNull(loaded);
        Assert.Equal(2.5, loaded!.Quantity);
        Assert.Equal(product.Id, created.ProductId);
        Assert.Equal(material.Id, created.MaterialId);
    }

    [Fact]
    public async Task CreateRecipeAsync_duplicate_throws()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var (product, material) = TestDbContextFactory.SeedProductAndMaterial(db);
        var sut = TestDbContextFactory.CreateRecipeService(db);
        await sut.CreateRecipeAsync(new Recipe
        {
            ProductId = product.Id,
            MaterialId = material.Id,
            Quantity = 1
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateRecipeAsync(new Recipe
            {
                ProductId = product.Id,
                MaterialId = material.Id,
                Quantity = 2
            }));
    }

    [Fact]
    public async Task GetRecipeByProductAsync_filters_by_product()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var (p1, m1) = TestDbContextFactory.SeedProductAndMaterial(db);
        var p2 = new Product { Name = "P2", MeasuringUnit = "шт", IsActive = true };
        db.Products.Add(p2);
        await db.SaveChangesAsync();

        var sut = TestDbContextFactory.CreateRecipeService(db);
        await sut.CreateRecipeAsync(new Recipe { ProductId = p1.Id, MaterialId = m1.Id, Quantity = 1 });
        await sut.CreateRecipeAsync(new Recipe { ProductId = p2.Id, MaterialId = m1.Id, Quantity = 3 });

        var forP1 = (await sut.GetRecipeByProductAsync(p1.Id)).ToList();

        Assert.Single(forP1);
        Assert.Equal(p1.Id, forP1[0].ProductId);
    }
}
