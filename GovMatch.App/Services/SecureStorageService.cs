using System.Security.Cryptography;
using System.Text;

namespace GovMatch.App.Services;

public class SecureStorageService
{
    public static string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public static string DecryptString(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            var bytes = Convert.FromBase64String(encryptedText);
            var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return string.Empty;
        }
    }
}
