using Dapper;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class AppSettingsRepository
{
    private readonly DatabaseContext _context;

    public AppSettingsRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM AppSettings WHERE Id = 1";
        var result = await connection.QueryFirstOrDefaultAsync<AppSettingsDto>(sql);
        return result?.ToModel() ?? new AppSettings { Id = 1 };
    }

    public async Task<bool> UpdateSettingsAsync(AppSettings settings)
    {
        var connection = _context.GetConnection();
        var dto = AppSettingsDto.FromModel(settings);
        var sql = @"
            UPDATE AppSettings SET
                EncryptedApiKey = @EncryptedApiKey,
                Environment = @Environment,
                DailyRateLimitTier = @DailyRateLimitTier,
                EnablePrefetch = @EnablePrefetch,
                PrefetchThreshold = @PrefetchThreshold,
                EnableNotifications = @EnableNotifications,
                NotificationThreshold = @NotificationThreshold,
                StructuredWeight = @StructuredWeight,
                TextWeight = @TextWeight,
                AgencyWeight = @AgencyWeight
            WHERE Id = 1
        ";
        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }
}

internal class AppSettingsDto
{
    public int Id { get; set; }
    public string? EncryptedApiKey { get; set; }
    public string Environment { get; set; } = "Production";
    public int DailyRateLimitTier { get; set; }
    public int EnablePrefetch { get; set; }
    public int PrefetchThreshold { get; set; }
    public int EnableNotifications { get; set; }
    public int NotificationThreshold { get; set; }
    public double StructuredWeight { get; set; }
    public double TextWeight { get; set; }
    public double AgencyWeight { get; set; }

    public AppSettings ToModel()
    {
        return new AppSettings
        {
            Id = Id,
            EncryptedApiKey = EncryptedApiKey,
            Environment = Environment,
            DailyRateLimitTier = DailyRateLimitTier,
            EnablePrefetch = EnablePrefetch == 1,
            PrefetchThreshold = PrefetchThreshold,
            EnableNotifications = EnableNotifications == 1,
            NotificationThreshold = NotificationThreshold,
            StructuredWeight = StructuredWeight,
            TextWeight = TextWeight,
            AgencyWeight = AgencyWeight
        };
    }

    public static AppSettingsDto FromModel(AppSettings model)
    {
        return new AppSettingsDto
        {
            Id = model.Id,
            EncryptedApiKey = model.EncryptedApiKey,
            Environment = model.Environment,
            DailyRateLimitTier = model.DailyRateLimitTier,
            EnablePrefetch = model.EnablePrefetch ? 1 : 0,
            PrefetchThreshold = model.PrefetchThreshold,
            EnableNotifications = model.EnableNotifications ? 1 : 0,
            NotificationThreshold = model.NotificationThreshold,
            StructuredWeight = model.StructuredWeight,
            TextWeight = model.TextWeight,
            AgencyWeight = model.AgencyWeight
        };
    }
}
