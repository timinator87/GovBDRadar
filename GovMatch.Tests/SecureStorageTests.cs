using GovMatch.App.Services;
using Xunit;

namespace GovMatch.Tests;

public class SecureStorageTests
{
    [Fact]
    public void EncryptString_ThenDecrypt_ReturnsOriginal()
    {
        // Arrange
        var original = "test-api-key-12345";

        // Act
        var encrypted = SecureStorageService.EncryptString(original);
        var decrypted = SecureStorageService.DecryptString(encrypted);

        // Assert
        Assert.NotEqual(original, encrypted);
        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void EncryptString_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var original = "";

        // Act
        var encrypted = SecureStorageService.EncryptString(original);

        // Assert
        Assert.Equal("", encrypted);
    }

    [Fact]
    public void DecryptString_WithInvalidData_ReturnsEmpty()
    {
        // Arrange
        var invalid = "not-a-valid-encrypted-string";

        // Act
        var decrypted = SecureStorageService.DecryptString(invalid);

        // Assert
        Assert.Equal("", decrypted);
    }
}
