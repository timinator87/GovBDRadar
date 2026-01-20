using GovMatch.Integrations.SamGov.DTOs;
using GovMatch.Integrations.SamGov.Mappers;
using Xunit;

namespace GovMatch.Tests;

public class OpportunityMapperTests
{
    [Fact]
    public void ToOpportunity_MapsBasicFields()
    {
        // Arrange
        var dto = new OpportunityData
        {
            NoticeId = "ABC123",
            Title = "Test Opportunity",
            SolicitationNumber = "SOL-2024-001",
            PostedDate = "2024-01-15",
            Type = "Presolicitation",
            BaseType = "Presolicitation"
        };

        // Act
        var opportunity = dto.ToOpportunity();

        // Assert
        Assert.Equal("ABC123", opportunity.NoticeId);
        Assert.Equal("Test Opportunity", opportunity.Title);
        Assert.Equal("SOL-2024-001", opportunity.SolicitationNumber);
        Assert.Equal("Presolicitation", opportunity.Type);
    }

    [Fact]
    public void ToOpportunity_HandlesResponseDeadlineBothSpellings()
    {
        // Arrange
        var dto1 = new OpportunityData
        {
            NoticeId = "TEST1",
            Title = "Test",
            PostedDate = "2024-01-15",
            ResponseDeadLine = "2024-02-15"
        };

        var dto2 = new OpportunityData
        {
            NoticeId = "TEST2",
            Title = "Test",
            PostedDate = "2024-01-15",
            ResponseDeadline = "2024-02-15"
        };

        // Act
        var opp1 = dto1.ToOpportunity();
        var opp2 = dto2.ToOpportunity();

        // Assert
        Assert.NotNull(opp1.ResponseDeadline);
        Assert.NotNull(opp2.ResponseDeadline);
    }

    [Fact]
    public void ToOpportunity_HandlesActiveFlag()
    {
        // Arrange
        var dto1 = new OpportunityData
        {
            NoticeId = "TEST1",
            Title = "Test",
            PostedDate = "2024-01-15",
            Active = "Yes"
        };

        var dto2 = new OpportunityData
        {
            NoticeId = "TEST2",
            Title = "Test",
            PostedDate = "2024-01-15",
            Active = "No"
        };

        // Act
        var opp1 = dto1.ToOpportunity();
        var opp2 = dto2.ToOpportunity();

        // Assert
        Assert.True(opp1.Active);
        Assert.False(opp2.Active);
    }

    [Fact]
    public void ToOpportunity_SerializesPocToJson()
    {
        // Arrange
        var dto = new OpportunityData
        {
            NoticeId = "TEST",
            Title = "Test",
            PostedDate = "2024-01-15",
            PointOfContact = new List<PointOfContact>
            {
                new() { Email = "test@example.com", FullName = "John Doe", Type = "Primary" }
            }
        };

        // Act
        var opportunity = dto.ToOpportunity();

        // Assert
        Assert.NotNull(opportunity.PocJson);
        Assert.Contains("test@example.com", opportunity.PocJson);
    }

    [Fact]
    public void ToOpportunity_HandlesMissingNoticeId()
    {
        // Arrange
        var dto = new OpportunityData
        {
            Title = "Test",
            PostedDate = "2024-01-15"
        };

        // Act
        var opportunity = dto.ToOpportunity();

        // Assert
        Assert.NotNull(opportunity.NoticeId);
        Assert.NotEmpty(opportunity.NoticeId);
    }
}
