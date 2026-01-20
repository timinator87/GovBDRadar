using Dapper;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class OpportunityEventRepository
{
    private readonly DatabaseContext _context;

    public OpportunityEventRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OpportunityEvent>> GetEventsSinceAsync(DateTime cutoff)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM OpportunityEvents WHERE OccurredAtUtc >= @Cutoff ORDER BY OccurredAtUtc DESC";
        var results = await connection.QueryAsync<OpportunityEventDto>(sql, new { Cutoff = cutoff.ToString("O") });
        return results.Select(r => r.ToModel());
    }

    public async Task<IEnumerable<OpportunityEvent>> GetEventsByNoticeIdAsync(string noticeId)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM OpportunityEvents WHERE NoticeId = @NoticeId ORDER BY OccurredAtUtc DESC";
        var results = await connection.QueryAsync<OpportunityEventDto>(sql, new { NoticeId = noticeId });
        return results.Select(r => r.ToModel());
    }

    public async Task<int> InsertAsync(OpportunityEvent entity)
    {
        var connection = _context.GetConnection();
        var dto = OpportunityEventDto.FromModel(entity);
        var sql = @"
            INSERT INTO OpportunityEvents (
                NoticeId, EventType, OldScore, NewScore, OccurredAtUtc, ProfileVersionId
            ) VALUES (
                @NoticeId, @EventType, @OldScore, @NewScore, @OccurredAtUtc, @ProfileVersionId
            );
            SELECT last_insert_rowid();
        ";
        var id = await connection.ExecuteScalarAsync<int>(sql, dto);
        return id;
    }
}

internal class OpportunityEventDto
{
    public int Id { get; set; }
    public string NoticeId { get; set; } = "";
    public int EventType { get; set; }
    public int? OldScore { get; set; }
    public int? NewScore { get; set; }
    public string OccurredAtUtc { get; set; } = "";
    public int? ProfileVersionId { get; set; }

    public OpportunityEvent ToModel()
    {
        return new OpportunityEvent
        {
            Id = Id,
            NoticeId = NoticeId,
            EventType = (OpportunityEventType)EventType,
            OldScore = OldScore,
            NewScore = NewScore,
            OccurredAtUtc = DateTime.Parse(OccurredAtUtc),
            ProfileVersionId = ProfileVersionId
        };
    }

    public static OpportunityEventDto FromModel(OpportunityEvent model)
    {
        return new OpportunityEventDto
        {
            Id = model.Id,
            NoticeId = model.NoticeId,
            EventType = (int)model.EventType,
            OldScore = model.OldScore,
            NewScore = model.NewScore,
            OccurredAtUtc = model.OccurredAtUtc.ToString("O"),
            ProfileVersionId = model.ProfileVersionId
        };
    }
}
