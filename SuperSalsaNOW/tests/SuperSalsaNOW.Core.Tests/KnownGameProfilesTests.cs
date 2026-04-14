using SuperSalsaNOW.Core.GameProfiles;
using SuperSalsaNOW.Core.Models;

namespace SuperSalsaNOW.Core.Tests;

public class KnownGameProfilesTests
{
    [Fact]
    public void All_contains_elden_ring_with_expected_id()
    {
        var elden = Assert.Single(KnownGameProfiles.All, p => p.Id == GameProfileIds.EldenRing);
        Assert.Equal("Elden Ring", elden.DisplayName);
        Assert.Equal(1245620, elden.SteamAppId);
        Assert.Contains("eldenring.exe", elden.GameExecutableRelativePath, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(GameProfileIds.EldenRing)]
    [InlineData("ELDEN-RING")]
    [InlineData(" Elden-Ring ")]
    public void GetById_finds_profile_case_insensitive_and_trims(string id)
    {
        var found = KnownGameProfiles.GetById(id);
        Assert.NotNull(found);
        Assert.Equal(GameProfileIds.EldenRing, found.Id);
    }

    [Fact]
    public void GetById_returns_null_for_unknown()
    {
        Assert.Null(KnownGameProfiles.GetById("not-a-game"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetById_returns_null_for_null_or_whitespace(string? id)
    {
        Assert.Null(KnownGameProfiles.GetById(id));
    }
}
