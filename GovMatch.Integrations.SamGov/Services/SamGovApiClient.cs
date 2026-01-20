using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using GovMatch.Integrations.SamGov.DTOs;
using Polly;
using Polly.Retry;

namespace GovMatch.Integrations.SamGov.Services;

public class SamGovApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public SamGovApiClient(string apiKey, string environment = "Production")
    {
        _apiKey = apiKey;
        _baseUrl = environment == "Alpha"
            ? "https://api-alpha.sam.gov"
            : "https://api.sam.gov";

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests || (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to {outcome.Result?.StatusCode}");
                }
            );
    }

    public async Task<SamGovSearchResponse?> SearchOpportunitiesAsync(SamGovSearchRequest request)
    {
        var url = BuildSearchUrl(request);
        var sanitizedUrl = SanitizeUrlForLogging(url);
        Console.WriteLine($"Fetching: {sanitizedUrl}");

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.GetAsync(url);
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SamGovSearchResponse>();
        return result;
    }

    public async Task<string?> GetOpportunityDescriptionAsync(string descriptionUrl)
    {
        var url = $"{_baseUrl}{descriptionUrl}";
        if (!url.Contains("api_key="))
        {
            var separator = url.Contains('?') ? "&" : "?";
            url += $"{separator}api_key={_apiKey}";
        }

        var sanitizedUrl = SanitizeUrlForLogging(url);
        Console.WriteLine($"Fetching description: {sanitizedUrl}");

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.GetAsync(url);
        });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return null;
    }

    private string BuildSearchUrl(SamGovSearchRequest request)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["api_key"] = _apiKey;
        query["postedFrom"] = request.PostedFrom.ToString("MM/dd/yyyy");
        query["postedTo"] = request.PostedTo.ToString("MM/dd/yyyy");
        query["limit"] = request.Limit.ToString();
        query["offset"] = request.Offset.ToString();

        if (!string.IsNullOrWhiteSpace(request.PType))
            query["ptype"] = request.PType;

        if (!string.IsNullOrWhiteSpace(request.Title))
            query["title"] = request.Title;

        if (!string.IsNullOrWhiteSpace(request.SolNum))
            query["solnum"] = request.SolNum;

        if (!string.IsNullOrWhiteSpace(request.NoticeId))
            query["noticeid"] = request.NoticeId;

        if (!string.IsNullOrWhiteSpace(request.Ncode))
            query["ncode"] = request.Ncode;

        if (!string.IsNullOrWhiteSpace(request.Ccode))
            query["ccode"] = request.Ccode;

        if (!string.IsNullOrWhiteSpace(request.State))
            query["state"] = request.State;

        if (!string.IsNullOrWhiteSpace(request.Zip))
            query["zip"] = request.Zip;

        if (!string.IsNullOrWhiteSpace(request.OrganizationName))
            query["organizationName"] = request.OrganizationName;

        if (!string.IsNullOrWhiteSpace(request.OrganizationCode))
            query["organizationCode"] = request.OrganizationCode;

        if (request.RdlFrom.HasValue)
            query["rdlfrom"] = request.RdlFrom.Value.ToString("MM/dd/yyyy");

        if (request.RdlTo.HasValue)
            query["rdlto"] = request.RdlTo.Value.ToString("MM/dd/yyyy");

        return $"{_baseUrl}/opportunities/v2/search?{query}";
    }

    private string SanitizeUrlForLogging(string url)
    {
        return url.Replace(_apiKey, "***");
    }
}

public class SamGovSearchRequest
{
    public DateTime PostedFrom { get; set; }
    public DateTime PostedTo { get; set; }
    public int Limit { get; set; } = 100;
    public int Offset { get; set; } = 0;
    public string? PType { get; set; }
    public string? Title { get; set; }
    public string? SolNum { get; set; }
    public string? NoticeId { get; set; }
    public string? Ncode { get; set; }
    public string? Ccode { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? OrganizationName { get; set; }
    public string? OrganizationCode { get; set; }
    public DateTime? RdlFrom { get; set; }
    public DateTime? RdlTo { get; set; }
}
