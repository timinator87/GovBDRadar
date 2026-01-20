using Dapper;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class CompanyProfileRepository
{
    private readonly DatabaseContext _context;

    public CompanyProfileRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<CompanyProfile> GetProfileAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM CompanyProfile WHERE Id = 1";
        var result = await connection.QueryFirstOrDefaultAsync<CompanyProfile>(sql);
        return result ?? new CompanyProfile { Id = 1 };
    }

    public async Task<bool> UpdateProfileAsync(CompanyProfile profile)
    {
        var connection = _context.GetConnection();
        var sql = @"
            UPDATE CompanyProfile SET
                DisplayName = @DisplayName,
                CapabilityText = @CapabilityText,
                KeywordsCsv = @KeywordsCsv,
                NaicsCsv = @NaicsCsv,
                PscCsv = @PscCsv,
                TargetAgencyCodesCsv = @TargetAgencyCodesCsv,
                TargetAgencyNamesCsv = @TargetAgencyNamesCsv,
                SetAsidePreferencesCsv = @SetAsidePreferencesCsv,
                PastPerformanceText = @PastPerformanceText
            WHERE Id = 1
        ";
        var rows = await connection.ExecuteAsync(sql, profile);
        return rows > 0;
    }
}
