namespace GovMatch.Core.Models;

public class AppSettings
{
    public int Id { get; set; }
    public string? EncryptedApiKey { get; set; }
    public string Environment { get; set; } = "Production";
    public int DailyRateLimitTier { get; set; } = 10;
    public bool EnablePrefetch { get; set; } = false;
    public int PrefetchThreshold { get; set; } = 75;
    public bool EnableNotifications { get; set; } = true;
    public int NotificationThreshold { get; set; } = 80;
    public double StructuredWeight { get; set; } = 0.4;
    public double TextWeight { get; set; } = 0.6;
    public double AgencyWeight { get; set; } = 1.0;
}
