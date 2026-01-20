namespace GovMatch.Core.Models;

public class Digest
{
    public int Id { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public int? ProfileVersionId { get; set; }
    public string? DigestJson { get; set; }
    public DigestDeliveryMethod SentVia { get; set; }
    public DateTime? LastDigestCutoffUtc { get; set; }
}

public enum DigestDeliveryMethod
{
    None = 0,
    Email = 1,
    Toast = 2,
    Both = 3
}

public class DigestData
{
    public List<DigestItem> NewOpportunities { get; set; } = new();
    public List<DigestItem> UpdatedOpportunities { get; set; } = new();
    public List<DigestItem> NewlyRelevantOpportunities { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public DateTime? CutoffTime { get; set; }
}

public class DigestItem
{
    public required string NoticeId { get; set; }
    public required string Title { get; set; }
    public string? Agency { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? ResponseDeadline { get; set; }
    public int Score { get; set; }
    public int? ScoreDelta { get; set; }
    public string? WhyMatchedSummary { get; set; }
    public string? UiUrl { get; set; }
}
