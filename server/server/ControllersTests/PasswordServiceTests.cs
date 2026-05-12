using Microsoft.Extensions.Logging.Abstractions;
using server.Services;

namespace ControllersTests;

public class PasswordServiceTests
{
    private static PasswordService CreateSut() =>
        new(NullLogger<PasswordService>.Instance);

    [Fact]
    public void HashPassword_empty_returns_empty()
    {
        var sut = CreateSut();
        Assert.Equal(string.Empty, sut.HashPassword(""));
    }

    [Fact]
    public void VerifyPasswordWithUpgrade_empty_inputs_invalid()
    {
        var sut = CreateSut();
        var r1 = sut.VerifyPasswordWithUpgrade("", "$2a$12$abcdefghijklmnopqrstuv");
        Assert.False(r1.IsValid);

        var hash = sut.HashPassword("secret");
        var r2 = sut.VerifyPasswordWithUpgrade("", hash);
        Assert.False(r2.IsValid);
    }

    [Fact]
    public void VerifyPassword_roundtrip_for_bcrypt_hash()
    {
        var sut = CreateSut();
        var hash = sut.HashPassword("my-password");

        Assert.True(hash.StartsWith("$2", StringComparison.Ordinal));
        Assert.True(sut.VerifyPassword("my-password", hash));
        Assert.False(sut.VerifyPassword("wrong", hash));
    }

    [Fact]
    public void VerifyPasswordWithUpgrade_plain_legacy_still_verifies_and_requests_upgrade()
    {
        var sut = CreateSut();
        const string plain = "legacy-plain";
        var result = sut.VerifyPasswordWithUpgrade(plain, plain);

        Assert.True(result.IsValid);
        Assert.True(result.NeedsUpgrade);
        Assert.False(string.IsNullOrEmpty(result.NewHash));
        Assert.StartsWith("$2", result.NewHash!, StringComparison.Ordinal);
    }

    [Fact]
    public void IsLegacyPassword_detects_non_bcrypt()
    {
        var sut = CreateSut();
        Assert.True(sut.IsLegacyPassword("plain"));
        Assert.False(sut.IsLegacyPassword(sut.HashPassword("x")));
    }
}
