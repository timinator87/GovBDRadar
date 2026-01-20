namespace GovMatch.Core.Models;

public class RetrievalRule
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool Enabled { get; set; } = true;
    public int DaysBack { get; set; } = 3;
    public string? PType { get; set; }
    public string? TitleKeywords { get; set; }
    public string? NaicsCodes { get; set; }
    public string? ClassificationCodes { get; set; }
    public string? AgencyNames { get; set; }
    public string? AgencyCodes { get; set; }
    public string? SetAsideCodes { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public DateTime? ResponseDeadlineFrom { get; set; }
    public DateTime? ResponseDeadlineTo { get; set; }
    public int MaxPages { get; set; } = 10;
    public int RequestBudget { get; set; } = 50;
    public DateTime? LastRunAtUtc { get; set; }
}
