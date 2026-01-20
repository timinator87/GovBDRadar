namespace GovMatch.Core.Models;

public class RateLimitRequest
{
    public int Id { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string? Endpoint { get; set; }
}
