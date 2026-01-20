using Dapper;
using GovMatch.Core.Models;

namespace GovMatch.Data.Services;

public class RateLimiter
{
    private readonly DatabaseContext _context;
    private readonly int _dailyLimit;

    public RateLimiter(DatabaseContext context, int dailyLimit)
    {
        _context = context;
        _dailyLimit = dailyLimit;
    }

    public async Task<bool> CanMakeRequestAsync()
    {
        var count = await GetRequestCountLast24HoursAsync();
        return count < _dailyLimit;
    }

    public async Task<int> GetRequestCountLast24HoursAsync()
    {
        var connection = _context.GetConnection();
        var cutoff = DateTime.UtcNow.AddHours(-24).ToString("O");
        var sql = "SELECT COUNT(*) FROM RateLimitRequests WHERE RequestedAtUtc >= @Cutoff";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Cutoff = cutoff });
        return count;
    }

    public async Task<int> GetRemainingRequestsAsync()
    {
        var used = await GetRequestCountLast24HoursAsync();
        return Math.Max(0, _dailyLimit - used);
    }

    public async Task RecordRequestAsync(string? endpoint = null)
    {
        var connection = _context.GetConnection();
        var sql = @"
            INSERT INTO RateLimitRequests (RequestedAtUtc, Endpoint)
            VALUES (@RequestedAtUtc, @Endpoint)
        ";
        await connection.ExecuteAsync(sql, new
        {
            RequestedAtUtc = DateTime.UtcNow.ToString("O"),
            Endpoint = endpoint
        });

        await CleanupOldRequestsAsync();
    }

    private async Task CleanupOldRequestsAsync()
    {
        var connection = _context.GetConnection();
        var cutoff = DateTime.UtcNow.AddDays(-2).ToString("O");
        var sql = "DELETE FROM RateLimitRequests WHERE RequestedAtUtc < @Cutoff";
        await connection.ExecuteAsync(sql, new { Cutoff = cutoff });
    }
}
