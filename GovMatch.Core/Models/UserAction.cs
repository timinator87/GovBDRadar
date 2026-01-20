namespace GovMatch.Core.Models;

public class UserAction
{
    public int Id { get; set; }
    public required string NoticeId { get; set; }
    public UserActionType ActionType { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public enum UserActionType
{
    None = 0,
    Saved = 1,
    Pursue = 2,
    Ignore = 3
}
