using Dapper;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class ProfileVersionRepository
{
    private readonly DatabaseContext _context;

    public ProfileVersionRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<ProfileVersion?> GetByIdAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM ProfileVersions WHERE Id = @Id";
        var result = await connection.QueryFirstOrDefaultAsync<ProfileVersionDto>(sql, new { Id = id });
        return result?.ToModel();
    }

    public async Task<IEnumerable<ProfileVersion>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM ProfileVersions ORDER BY CreatedAtUtc DESC";
        var results = await connection.QueryAsync<ProfileVersionDto>(sql);
        return results.Select(r => r.ToModel());
    }

    public async Task<ProfileVersion?> GetLatestAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM ProfileVersions ORDER BY CreatedAtUtc DESC LIMIT 1";
        var result = await connection.QueryFirstOrDefaultAsync<ProfileVersionDto>(sql);
        return result?.ToModel();
    }

    public async Task<int> InsertAsync(ProfileVersion entity)
    {
        var connection = _context.GetConnection();
        var dto = ProfileVersionDto.FromModel(entity);
        var sql = @"
            INSERT INTO ProfileVersions (
                CreatedAtUtc, DisplayNameSnapshot, CapabilityTextSnapshot,
                KeywordsSnapshot, NaicsSnapshot, PscSnapshot,
                TargetAgencyCodesSnapshot, SetAsidePreferencesSnapshot, PastPerformanceTextSnapshot
            ) VALUES (
                @CreatedAtUtc, @DisplayNameSnapshot, @CapabilityTextSnapshot,
                @KeywordsSnapshot, @NaicsSnapshot, @PscSnapshot,
                @TargetAgencyCodesSnapshot, @SetAsidePreferencesSnapshot, @PastPerformanceTextSnapshot
            );
            SELECT last_insert_rowid();
        ";
        var id = await connection.ExecuteScalarAsync<int>(sql, dto);
        return id;
    }
}

internal class ProfileVersionDto
{
    public int Id { get; set; }
    public string CreatedAtUtc { get; set; } = "";
    public string DisplayNameSnapshot { get; set; } = "";
    public string? CapabilityTextSnapshot { get; set; }
    public string? KeywordsSnapshot { get; set; }
    public string? NaicsSnapshot { get; set; }
    public string? PscSnapshot { get; set; }
    public string? TargetAgencyCodesSnapshot { get; set; }
    public string? SetAsidePreferencesSnapshot { get; set; }
    public string? PastPerformanceTextSnapshot { get; set; }

    public ProfileVersion ToModel()
    {
        return new ProfileVersion
        {
            Id = Id,
            CreatedAtUtc = DateTime.Parse(CreatedAtUtc),
            DisplayNameSnapshot = DisplayNameSnapshot,
            CapabilityTextSnapshot = CapabilityTextSnapshot,
            KeywordsSnapshot = KeywordsSnapshot,
            NaicsSnapshot = NaicsSnapshot,
            PscSnapshot = PscSnapshot,
            TargetAgencyCodesSnapshot = TargetAgencyCodesSnapshot,
            SetAsidePreferencesSnapshot = SetAsidePreferencesSnapshot,
            PastPerformanceTextSnapshot = PastPerformanceTextSnapshot
        };
    }

    public static ProfileVersionDto FromModel(ProfileVersion model)
    {
        return new ProfileVersionDto
        {
            Id = model.Id,
            CreatedAtUtc = model.CreatedAtUtc.ToString("O"),
            DisplayNameSnapshot = model.DisplayNameSnapshot,
            CapabilityTextSnapshot = model.CapabilityTextSnapshot,
            KeywordsSnapshot = model.KeywordsSnapshot,
            NaicsSnapshot = model.NaicsSnapshot,
            PscSnapshot = model.PscSnapshot,
            TargetAgencyCodesSnapshot = model.TargetAgencyCodesSnapshot,
            SetAsidePreferencesSnapshot = model.SetAsidePreferencesSnapshot,
            PastPerformanceTextSnapshot = model.PastPerformanceTextSnapshot
        };
    }
}
