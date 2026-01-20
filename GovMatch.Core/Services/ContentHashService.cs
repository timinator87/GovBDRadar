using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GovMatch.Core.Services;

public static class ContentHashService
{
    public static string ComputeHash(string rawJson, string? descriptionText = null)
    {
        var canonicalJson = CanonicalizeJson(rawJson);
        var combined = descriptionText != null
            ? canonicalJson + "|DESC:" + descriptionText
            : canonicalJson;

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hashBytes);
    }

    private static string CanonicalizeJson(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = null
            };

            var element = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonSerializer.Serialize(element, options);
        }
        catch
        {
            return json;
        }
    }
}
