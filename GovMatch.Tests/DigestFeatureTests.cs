using GovMatch.Core.Services;
using Xunit;

namespace GovMatch.Tests;

public class DigestFeatureTests
{
    [Fact]
    public void ContentHashService_ComputesStableHash()
    {
        // Arrange
        var json1 = "{\"title\":\"Test\",\"id\":\"123\"}";
        var json2 = "{\"id\":\"123\",\"title\":\"Test\"}";

        // Act
        var hash1 = ContentHashService.ComputeHash(json1);
        var hash2 = ContentHashService.ComputeHash(json2);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ContentHashService_ChangeDetection()
    {
        // Arrange
        var json1 = "{\"title\":\"Original Title\",\"id\":\"123\"}";
        var json2 = "{\"title\":\"Updated Title\",\"id\":\"123\"}";

        // Act
        var hash1 = ContentHashService.ComputeHash(json1);
        var hash2 = ContentHashService.ComputeHash(json2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ContentHashService_IncludesDescription()
    {
        // Arrange
        var json = "{\"title\":\"Test\",\"id\":\"123\"}";
        var desc1 = "Original description";
        var desc2 = "Updated description";

        // Act
        var hash1 = ContentHashService.ComputeHash(json, desc1);
        var hash2 = ContentHashService.ComputeHash(json, desc2);
        var hashNoDesc = ContentHashService.ComputeHash(json);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hashNoDesc);
    }

    [Fact]
    public void ContentHashService_HandlesNullDescription()
    {
        // Arrange
        var json = "{\"title\":\"Test\"}";

        // Act
        var hash1 = ContentHashService.ComputeHash(json, null);
        var hash2 = ContentHashService.ComputeHash(json);

        // Assert
        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
    }

    [Fact]
    public void ContentHashService_HandlesMalformedJson()
    {
        // Arrange
        var badJson = "not valid json {";

        // Act
        var hash = ContentHashService.ComputeHash(badJson);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }
}
