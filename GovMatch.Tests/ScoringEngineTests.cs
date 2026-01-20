using GovMatch.Core.Models;
using GovMatch.Core.Services;
using Xunit;

namespace GovMatch.Tests;

public class ScoringEngineTests
{
    [Fact]
    public void ScoreOpportunity_WithMatchingNaics_ReturnsHigherScore()
    {
        // Arrange
        var engine = new TfIdfScoringEngine();
        var profile = new CompanyProfile
        {
            Id = 1,
            NaicsCsv = "541511,541512",
            CapabilityText = "Software development services"
        };
        var opportunity = new Opportunity
        {
            NoticeId = "TEST001",
            Title = "Software Development Contract",
            PostedDate = DateTime.UtcNow,
            NaicsCode = "541511",
            LastSeenAtUtc = DateTime.UtcNow,
            FirstSeenAtUtc = DateTime.UtcNow
        };

        // Act
        var result = engine.ScoreOpportunity(profile, opportunity);

        // Assert
        Assert.True(result.Score > 0);
        Assert.NotNull(result.ScoreBreakdownJson);
    }

    [Fact]
    public void ScoreOpportunity_WithNoMatches_ReturnsLowScore()
    {
        // Arrange
        var engine = new TfIdfScoringEngine();
        var profile = new CompanyProfile
        {
            Id = 1,
            NaicsCsv = "111111",
            CapabilityText = "Agriculture services"
        };
        var opportunity = new Opportunity
        {
            NoticeId = "TEST002",
            Title = "IT Support Services",
            PostedDate = DateTime.UtcNow,
            NaicsCode = "541519",
            LastSeenAtUtc = DateTime.UtcNow,
            FirstSeenAtUtc = DateTime.UtcNow
        };

        // Act
        var result = engine.ScoreOpportunity(profile, opportunity);

        // Assert
        Assert.True(result.Score >= 0);
    }

    [Fact]
    public void ScoreOpportunity_WithMatchingPsc_IncludesPscPoints()
    {
        // Arrange
        var engine = new TfIdfScoringEngine();
        var profile = new CompanyProfile
        {
            Id = 1,
            PscCsv = "R408,D302",
            CapabilityText = "IT services"
        };
        var opportunity = new Opportunity
        {
            NoticeId = "TEST003",
            Title = "IT Contract",
            PostedDate = DateTime.UtcNow,
            ClassificationCode = "R408",
            LastSeenAtUtc = DateTime.UtcNow,
            FirstSeenAtUtc = DateTime.UtcNow
        };

        // Act
        var result = engine.ScoreOpportunity(profile, opportunity);

        // Assert
        Assert.True(result.Score > 0);
    }

    [Fact]
    public void ScoreOpportunity_WithMatchingSetAside_IncludesSetAsidePoints()
    {
        // Arrange
        var engine = new TfIdfScoringEngine();
        var profile = new CompanyProfile
        {
            Id = 1,
            SetAsidePreferencesCsv = "SBA,8A",
            CapabilityText = "Consulting"
        };
        var opportunity = new Opportunity
        {
            NoticeId = "TEST004",
            Title = "Consulting Services",
            PostedDate = DateTime.UtcNow,
            SetAsideCode = "8A",
            SetAsideDescription = "8(a) Set Aside",
            LastSeenAtUtc = DateTime.UtcNow,
            FirstSeenAtUtc = DateTime.UtcNow
        };

        // Act
        var result = engine.ScoreOpportunity(profile, opportunity);

        // Assert
        Assert.True(result.Score > 0);
    }

    [Fact]
    public void ScoreOpportunity_WithTextSimilarity_ReturnsTextScore()
    {
        // Arrange
        var engine = new TfIdfScoringEngine();
        var profile = new CompanyProfile
        {
            Id = 1,
            CapabilityText = "Cloud computing infrastructure services AWS Azure deployment migration"
        };
        var opportunity = new Opportunity
        {
            NoticeId = "TEST005",
            Title = "Cloud Infrastructure Migration Services",
            PostedDate = DateTime.UtcNow,
            LastSeenAtUtc = DateTime.UtcNow,
            FirstSeenAtUtc = DateTime.UtcNow
        };

        // Act
        var result = engine.ScoreOpportunity(profile, opportunity, "Cloud computing migration to AWS and Azure infrastructure");

        // Assert
        Assert.True(result.Score > 20);
    }

    [Fact]
    public void ScoreOpportunity_ScoreNeverExceeds100()
    {
        // Arrange
        var engine = new TfIdfScoringEngine();
        var profile = new CompanyProfile
        {
            Id = 1,
            NaicsCsv = "541511",
            PscCsv = "R408",
            SetAsidePreferencesCsv = "8A",
            TargetAgencyCodesCsv = "7000",
            CapabilityText = "Software development cloud computing infrastructure services"
        };
        var opportunity = new Opportunity
        {
            NoticeId = "TEST006",
            Title = "Software Development Services",
            PostedDate = DateTime.UtcNow,
            NaicsCode = "541511",
            ClassificationCode = "R408",
            SetAsideCode = "8A",
            AgencyPathCode = "7000.1234",
            LastSeenAtUtc = DateTime.UtcNow,
            FirstSeenAtUtc = DateTime.UtcNow
        };

        // Act
        var result = engine.ScoreOpportunity(profile, opportunity, "Software development cloud computing infrastructure services");

        // Assert
        Assert.True(result.Score <= 100);
        Assert.True(result.Score >= 0);
    }
}
