namespace GovMatch.Core.Models;

public class OpportunityEvent
{
    public int Id { get; set; }
    public required string NoticeId { get; set; }
    public OpportunityEventType EventType { get; set; }
    public int? OldScore { get; set; }
    public int? NewScore { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int? ProfileVersionId { get; set; }
}

public enum OpportunityEventType
{
    New = 1,
    Updated = 2,
    ScoreUp = 3,
    ScoreDown = 4
}
