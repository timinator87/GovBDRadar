namespace GovMatch.Core.Models;

public class Opportunity
{
    public int Id { get; set; }
    public required string NoticeId { get; set; }
    public required string Title { get; set; }
    public string? SolicitationNumber { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? ResponseDeadline { get; set; }
    public string? Type { get; set; }
    public string? BaseType { get; set; }
    public bool Active { get; set; }
    public string? NaicsCode { get; set; }
    public string? ClassificationCode { get; set; }
    public string? SetAsideCode { get; set; }
    public string? SetAsideDescription { get; set; }
    public string? AgencyPathName { get; set; }
    public string? AgencyPathCode { get; set; }
    public string? PlaceOfPerformance { get; set; }
    public string? PocJson { get; set; }
    public string? DescriptionUrl { get; set; }
    public string? DescriptionText { get; set; }
    public string? UiUrl { get; set; }
    public string? ResourceLinksJson { get; set; }
    public string? RawJson { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public string? ContentHash { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastFetchedAtUtc { get; set; }
}
