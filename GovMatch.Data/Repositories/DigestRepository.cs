using Dapper;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class DigestRepository
{
    private readonly DatabaseContext _context;

    public DigestRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Digest?> GetLatestAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM Digests ORDER BY GeneratedAtUtc DESC LIMIT 1";
        var result = await connection.QueryFirstOrDefaultAsync<DigestDto>(sql);
        return result?.ToModel();
    }

    public async Task<IEnumerable<Digest>> GetRecentAsync(int count = 10)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM Digests ORDER BY GeneratedAtUtc DESC LIMIT @Count";
        var results = await connection.QueryAsync<DigestDto>(sql, new { Count = count });
        return results.Select(r => r.ToModel());
    }

    public async Task<int> InsertAsync(Digest entity)
    {
        var connection = _context.GetConnection();
        var dto = DigestDto.FromModel(entity);
        var sql = @"
            INSERT INTO Digests (
                GeneratedAtUtc, ProfileVersionId, DigestJson, SentVia, LastDigestCutoffUtc
            ) VALUES (
                @GeneratedAtUtc, @ProfileVersionId, @DigestJson, @SentVia, @LastDigestCutoffUtc
            );
            SELECT last_insert_rowid();
        ";
        var id = await connection.ExecuteScalarAsync<int>(sql, dto);
        return id;
    }
}

internal class DigestDto
{
    public int Id { get; set; }
    public string GeneratedAtUtc { get; set; } = "";
    public int? ProfileVersionId { get; set; }
    public string? DigestJson { get; set; }
    public int SentVia { get; set; }
    public string? LastDigestCutoffUtc { get; set; }

    public Digest ToModel()
    {
        return new Digest
        {
            Id = Id,
            GeneratedAtUtc = DateTime.Parse(GeneratedAtUtc),
            ProfileVersionId = ProfileVersionId,
            DigestJson = DigestJson,
            SentVia = (DigestDeliveryMethod)SentVia,
            LastDigestCutoffUtc = string.IsNullOrWhiteSpace(LastDigestCutoffUtc) ? null : DateTime.Parse(LastDigestCutoffUtc)
        };
    }

    public static DigestDto FromModel(Digest model)
    {
        return new DigestDto
        {
            Id = model.Id,
            GeneratedAtUtc = model.GeneratedAtUtc.ToString("O"),
            ProfileVersionId = model.ProfileVersionId,
            DigestJson = model.DigestJson,
            SentVia = (int)model.SentVia,
            LastDigestCutoffUtc = model.LastDigestCutoffUtc?.ToString("O")
        };
    }
}
