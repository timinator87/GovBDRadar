using GovMatch.Core.Interfaces;
using GovMatch.Core.Models;
using GovMatch.Data.Repositories;
using GovMatch.Data.Services;
using GovMatch.Integrations.SamGov.Mappers;

namespace GovMatch.Integrations.SamGov.Services;

public class SyncService
{
    private readonly SamGovApiClient _apiClient;
    private readonly OpportunityRepository _opportunityRepo;
    private readonly MatchResultRepository _matchResultRepo;
    private readonly CompanyProfileRepository _profileRepo;
    private readonly RetrievalRuleRepository _ruleRepo;
    private readonly RateLimiter _rateLimiter;
    private readonly IScoringEngine _scoringEngine;

    public SyncService(
        SamGovApiClient apiClient,
        OpportunityRepository opportunityRepo,
        MatchResultRepository matchResultRepo,
        CompanyProfileRepository profileRepo,
        RetrievalRuleRepository ruleRepo,
        RateLimiter rateLimiter,
        IScoringEngine scoringEngine)
    {
        _apiClient = apiClient;
        _opportunityRepo = opportunityRepo;
        _matchResultRepo = matchResultRepo;
        _profileRepo = profileRepo;
        _ruleRepo = ruleRepo;
        _rateLimiter = rateLimiter;
        _scoringEngine = scoringEngine;
    }

    public async Task<SyncResult> RunSyncAsync()
    {
        var result = new SyncResult();
        var profile = await _profileRepo.GetProfileAsync();
        var rules = await _ruleRepo.GetEnabledRulesAsync();

        foreach (var rule in rules)
        {
            if (!await _rateLimiter.CanMakeRequestAsync())
            {
                result.Errors.Add($"Rate limit exceeded. Cannot run rule: {rule.Name}");
                break;
            }

            var ruleResult = await RunRuleAsync(rule, profile);
            result.OpportunitiesFetched += ruleResult.OpportunitiesFetched;
            result.NewOpportunities += ruleResult.NewOpportunities;
            result.RequestsMade += ruleResult.RequestsMade;
            result.Errors.AddRange(ruleResult.Errors);
        }

        return result;
    }

    private async Task<SyncResult> RunRuleAsync(RetrievalRule rule, CompanyProfile profile)
    {
        var result = new SyncResult();
        var seenNoticeIds = new HashSet<string>();

        var postedFrom = DateTime.UtcNow.AddDays(-rule.DaysBack);
        var postedTo = DateTime.UtcNow;

        for (int page = 0; page < rule.MaxPages; page++)
        {
            if (result.RequestsMade >= rule.RequestBudget)
                break;

            if (!await _rateLimiter.CanMakeRequestAsync())
            {
                result.Errors.Add($"Rate limit exceeded for rule: {rule.Name}");
                break;
            }

            try
            {
                var searchRequest = new SamGovSearchRequest
                {
                    PostedFrom = postedFrom,
                    PostedTo = postedTo,
                    Limit = 100,
                    Offset = page * 100,
                    PType = rule.PType,
                    Title = rule.TitleKeywords,
                    Ncode = rule.NaicsCodes,
                    Ccode = rule.ClassificationCodes,
                    OrganizationCode = rule.AgencyCodes,
                    State = rule.State,
                    Zip = rule.Zip,
                    RdlFrom = rule.ResponseDeadlineFrom,
                    RdlTo = rule.ResponseDeadlineTo
                };

                var response = await _apiClient.SearchOpportunitiesAsync(searchRequest);
                await _rateLimiter.RecordRequestAsync($"/opportunities/v2/search");
                result.RequestsMade++;

                if (response == null || response.OpportunitiesData.Count == 0)
                    break;

                result.OpportunitiesFetched += response.OpportunitiesData.Count;

                foreach (var oppData in response.OpportunitiesData)
                {
                    if (oppData.NoticeId == null || seenNoticeIds.Contains(oppData.NoticeId))
                        continue;

                    seenNoticeIds.Add(oppData.NoticeId);

                    var existing = await _opportunityRepo.GetByNoticeIdAsync(oppData.NoticeId);
                    var opportunity = oppData.ToOpportunity();

                    if (existing == null)
                    {
                        opportunity.Id = await _opportunityRepo.InsertAsync(opportunity);
                        result.NewOpportunities++;

                        var matchResult = _scoringEngine.ScoreOpportunity(profile, opportunity, null);
                        matchResult.Id = await _matchResultRepo.InsertAsync(matchResult);
                    }
                    else
                    {
                        opportunity.Id = existing.Id;
                        opportunity.FirstSeenAtUtc = existing.FirstSeenAtUtc;
                        await _opportunityRepo.UpdateAsync(opportunity);
                    }
                }

                if (response.OpportunitiesData.Count < 100)
                    break;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error fetching page {page} for rule {rule.Name}: {ex.Message}");
                break;
            }
        }

        rule.LastRunAtUtc = DateTime.UtcNow;
        await _ruleRepo.UpdateAsync(rule);

        return result;
    }
}

public class SyncResult
{
    public int OpportunitiesFetched { get; set; }
    public int NewOpportunities { get; set; }
    public int RequestsMade { get; set; }
    public List<string> Errors { get; set; } = new();
}
