namespace GovMatch.Core.Models;

public class MatchResult
{
    public int Id { get; set; }
    public required string NoticeId { get; set; }
    public int Score { get; set; }
    public string? ScoreBreakdownJson { get; set; }
    public DateTime ScoredAtUtc { get; set; }
}

public class ScoreBreakdown
{
    public int TotalScore { get; set; }
    public List<ScoreFactor> Factors { get; set; } = new();
}

public class ScoreFactor
{
    public required string Name { get; set; }
    public int Points { get; set; }
    public int MaxPoints { get; set; }
    public string? Details { get; set; }
}
