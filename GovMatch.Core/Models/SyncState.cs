namespace GovMatch.Core.Models;

public class SyncState
{
    public int Id { get; set; }
    public DateTime? LastSyncAtUtc { get; set; }
    public DateTime? LastPostedToDateUsed { get; set; }
    public DateTime? LastPostedFromDateUsed { get; set; }
    public string? LastError { get; set; }
    public int RequestsLast24Hours { get; set; }
    public DateTime? RequestCounterResetAtUtc { get; set; }
}
