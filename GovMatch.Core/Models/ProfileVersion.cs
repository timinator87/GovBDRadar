namespace GovMatch.Core.Models;

public class ProfileVersion
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string DisplayNameSnapshot { get; set; } = "";
    public string? CapabilityTextSnapshot { get; set; }
    public string? KeywordsSnapshot { get; set; }
    public string? NaicsSnapshot { get; set; }
    public string? PscSnapshot { get; set; }
    public string? TargetAgencyCodesSnapshot { get; set; }
    public string? SetAsidePreferencesSnapshot { get; set; }
    public string? PastPerformanceTextSnapshot { get; set; }
}
