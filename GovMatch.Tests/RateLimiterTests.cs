using GovMatch.Data;
using GovMatch.Data.Services;
using Xunit;

namespace GovMatch.Tests;

public class RateLimiterTests
{
    private DatabaseContext CreateTestDatabase()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        var context = new DatabaseContext(dbPath);
        context.InitializeDatabaseAsync().Wait();
        return context;
    }

    [Fact]
    public async Task CanMakeRequest_WithinLimit_ReturnsTrue()
    {
        // Arrange
        using var context = CreateTestDatabase();
        var limiter = new RateLimiter(context, dailyLimit: 10);

        // Act
        var canMake = await limiter.CanMakeRequestAsync();

        // Assert
        Assert.True(canMake);
    }

    [Fact]
    public async Task RecordRequest_IncrementsCount()
    {
        // Arrange
        using var context = CreateTestDatabase();
        var limiter = new RateLimiter(context, dailyLimit: 10);

        // Act
        await limiter.RecordRequestAsync("/test");
        var count = await limiter.GetRequestCountLast24HoursAsync();

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetRemainingRequests_CalculatesCorrectly()
    {
        // Arrange
        using var context = CreateTestDatabase();
        var limiter = new RateLimiter(context, dailyLimit: 10);

        // Act
        await limiter.RecordRequestAsync("/test1");
        await limiter.RecordRequestAsync("/test2");
        await limiter.RecordRequestAsync("/test3");
        var remaining = await limiter.GetRemainingRequestsAsync();

        // Assert
        Assert.Equal(7, remaining);
    }

    [Fact]
    public async Task CanMakeRequest_AtLimit_ReturnsFalse()
    {
        // Arrange
        using var context = CreateTestDatabase();
        var limiter = new RateLimiter(context, dailyLimit: 3);

        // Act
        await limiter.RecordRequestAsync("/test1");
        await limiter.RecordRequestAsync("/test2");
        await limiter.RecordRequestAsync("/test3");
        var canMake = await limiter.CanMakeRequestAsync();

        // Assert
        Assert.False(canMake);
    }
}
