using GovMatch.Core.Models;
using Xunit;

namespace GovMatch.Tests;

public class CompanyProfileTests
{
    [Fact]
    public void GetKeywords_ReturnsEmptyForNull()
    {
        // Arrange
        var profile = new CompanyProfile { Id = 1, KeywordsCsv = null };

        // Act
        var keywords = profile.GetKeywords();

        // Assert
        Assert.Empty(keywords);
    }

    [Fact]
    public void GetKeywords_SplitsCommaSeparated()
    {
        // Arrange
        var profile = new CompanyProfile { Id = 1, KeywordsCsv = "cloud,software,development" };

        // Act
        var keywords = profile.GetKeywords().ToList();

        // Assert
        Assert.Equal(3, keywords.Count);
        Assert.Contains("cloud", keywords);
        Assert.Contains("software", keywords);
        Assert.Contains("development", keywords);
    }

    [Fact]
    public void GetNaicsCodes_SplitsAndTrims()
    {
        // Arrange
        var profile = new CompanyProfile { Id = 1, NaicsCsv = "541511, 541512, 541519" };

        // Act
        var codes = profile.GetNaicsCodes().ToList();

        // Assert
        Assert.Equal(3, codes.Count);
        Assert.All(codes, code => Assert.DoesNotContain(" ", code));
    }

    [Fact]
    public void GetCombinedProfileText_CombinesAllFields()
    {
        // Arrange
        var profile = new CompanyProfile
        {
            Id = 1,
            CapabilityText = "We provide IT services",
            PastPerformanceText = "Delivered 50 projects",
            KeywordsCsv = "cloud,software"
        };

        // Act
        var combined = profile.GetCombinedProfileText();

        // Assert
        Assert.Contains("IT services", combined);
        Assert.Contains("50 projects", combined);
        Assert.Contains("cloud", combined);
        Assert.Contains("software", combined);
    }

    [Fact]
    public void GetSetAsidePreferences_HandlesEmptyString()
    {
        // Arrange
        var profile = new CompanyProfile { Id = 1, SetAsidePreferencesCsv = "" };

        // Act
        var preferences = profile.GetSetAsidePreferences();

        // Assert
        Assert.Empty(preferences);
    }
}
