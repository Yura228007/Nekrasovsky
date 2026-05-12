using server.Models;

namespace ControllersTests;

public class UserPermissionsServiceTests
{
    [Fact]
    public async Task AddPermissionToUserAsync_throws_when_user_not_found()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var permission = TestDbContextFactory.SeedPermission(db);
        var sut = TestDbContextFactory.CreateUserPermissionsService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.AddPermissionToUserAsync(userId: 999, permission.Id));
    }

    [Fact]
    public async Task AddPermissionToUserAsync_throws_when_permission_not_found()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var user = TestDbContextFactory.SeedUser(db);
        var sut = TestDbContextFactory.CreateUserPermissionsService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.AddPermissionToUserAsync(user.Id, permissionId: 999));
    }

    [Fact]
    public async Task AddPermissionToUserAsync_adds_link()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var user = TestDbContextFactory.SeedUser(db);
        var permission = TestDbContextFactory.SeedPermission(db);
        var sut = TestDbContextFactory.CreateUserPermissionsService(db);

        await sut.AddPermissionToUserAsync(user.Id, permission.Id);

        Assert.True(await sut.HasPermissionAsync(user.Id, permission.Code));
    }

    [Fact]
    public async Task AddPermissionToUserAsync_duplicate_throws()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var user = TestDbContextFactory.SeedUser(db);
        var permission = TestDbContextFactory.SeedPermission(db);
        var sut = TestDbContextFactory.CreateUserPermissionsService(db);
        await sut.AddPermissionToUserAsync(user.Id, permission.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddPermissionToUserAsync(user.Id, permission.Id));
    }

    [Fact]
    public async Task RemovePermissionFromUserAsync_removes_link()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var user = TestDbContextFactory.SeedUser(db);
        var permission = TestDbContextFactory.SeedPermission(db);
        var sut = TestDbContextFactory.CreateUserPermissionsService(db);
        await sut.AddPermissionToUserAsync(user.Id, permission.Id);

        await sut.RemovePermissionFromUserAsync(user.Id, permission.Id);

        Assert.False(await sut.HasPermissionAsync(user.Id, permission.Code));
    }

    [Fact]
    public async Task UpdateUserPermissionsAsync_replaces_all_permissions()
    {
        await using var db = TestDbContextFactory.CreateContext();
        var user = TestDbContextFactory.SeedUser(db);
        var p1 = TestDbContextFactory.SeedPermission(db, "A");
        var p2 = TestDbContextFactory.SeedPermission(db, "B");
        var sut = TestDbContextFactory.CreateUserPermissionsService(db);
        await sut.AddPermissionToUserAsync(user.Id, p1.Id);
        await sut.AddPermissionToUserAsync(user.Id, p2.Id);

        var p3 = TestDbContextFactory.SeedPermission(db, "C");
        await sut.UpdateUserPermissionsAsync(user.Id, new[] { p3.Id });

        var list = (await sut.GetUserPermissionsAsync(user.Id)).ToList();
        Assert.Single(list);
        Assert.Equal("C", list[0].Code);
    }
}
