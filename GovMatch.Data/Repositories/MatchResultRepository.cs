using Dapper;
using GovMatch.Core.Interfaces;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class MatchResultRepository : IRepository<MatchResult>
{
    private readonly DatabaseContext _context;

    public MatchResultRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<MatchResult?> GetByIdAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM MatchResults WHERE Id = @Id";
        var result = await connection.QueryFirstOrDefaultAsync<MatchResultDto>(sql, new { Id = id });
        return result?.ToModel();
    }

    public async Task<MatchResult?> GetByNoticeIdAsync(string noticeId)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM MatchResults WHERE NoticeId = @NoticeId ORDER BY ScoredAtUtc DESC LIMIT 1";
        var result = await connection.QueryFirstOrDefaultAsync<MatchResultDto>(sql, new { NoticeId = noticeId });
        return result?.ToModel();
    }

    public async Task<IEnumerable<MatchResult>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM MatchResults ORDER BY Score DESC";
        var results = await connection.QueryAsync<MatchResultDto>(sql);
        return results.Select(r => r.ToModel());
    }

    public async Task<int> InsertAsync(MatchResult entity)
    {
        var connection = _context.GetConnection();
        var dto = MatchResultDto.FromModel(entity);
        var sql = @"
            INSERT INTO MatchResults (NoticeId, Score, ScoreBreakdownJson, ScoredAtUtc)
            VALUES (@NoticeId, @Score, @ScoreBreakdownJson, @ScoredAtUtc);
            SELECT last_insert_rowid();
        ";
        var id = await connection.ExecuteScalarAsync<int>(sql, dto);
        return id;
    }

    public async Task<bool> UpdateAsync(MatchResult entity)
    {
        var connection = _context.GetConnection();
        var dto = MatchResultDto.FromModel(entity);
        var sql = @"
            UPDATE MatchResults SET
                Score = @Score,
                ScoreBreakdownJson = @ScoreBreakdownJson,
                ScoredAtUtc = @ScoredAtUtc
            WHERE Id = @Id
        ";
        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "DELETE FROM MatchResults WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}

internal class MatchResultDto
{
    public int Id { get; set; }
    public string NoticeId { get; set; } = "";
    public int Score { get; set; }
    public string? ScoreBreakdownJson { get; set; }
    public string ScoredAtUtc { get; set; } = "";

    public MatchResult ToModel()
    {
        return new MatchResult
        {
            Id = Id,
            NoticeId = NoticeId,
            Score = Score,
            ScoreBreakdownJson = ScoreBreakdownJson,
            ScoredAtUtc = DateTime.Parse(ScoredAtUtc)
        };
    }

    public static MatchResultDto FromModel(MatchResult model)
    {
        return new MatchResultDto
        {
            Id = model.Id,
            NoticeId = model.NoticeId,
            Score = model.Score,
            ScoreBreakdownJson = model.ScoreBreakdownJson,
            ScoredAtUtc = model.ScoredAtUtc.ToString("O")
        };
    }
}
