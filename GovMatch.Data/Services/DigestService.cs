using System.Text.Json;
using GovMatch.Core.Models;
using GovMatch.Data.Repositories;

namespace GovMatch.Data.Services;

public class DigestService
{
    private readonly OpportunityRepository _opportunityRepo;
    private readonly OpportunityEventRepository _eventRepo;
    private readonly MatchResultRepository _matchResultRepo;
    private readonly DigestRepository _digestRepo;
    private readonly ProfileVersionRepository _versionRepo;

    public DigestService(
        OpportunityRepository opportunityRepo,
        OpportunityEventRepository eventRepo,
        MatchResultRepository matchResultRepo,
        DigestRepository digestRepo,
        ProfileVersionRepository versionRepo)
    {
        _opportunityRepo = opportunityRepo;
        _eventRepo = eventRepo;
        _matchResultRepo = matchResultRepo;
        _digestRepo = digestRepo;
        _versionRepo = versionRepo;
    }

    public async Task<DigestData> GenerateDigestAsync(int topN = 20)
    {
        var lastDigest = await _digestRepo.GetLatestAsync();
        var cutoff = lastDigest?.LastDigestCutoffUtc ?? DateTime.UtcNow.AddDays(-1);

        var allOpportunities = (await _opportunityRepo.GetAllAsync()).ToList();

        var newOpps = allOpportunities
            .Where(o => o.FirstSeenAtUtc > cutoff)
            .OrderByDescending(o => o.FirstSeenAtUtc)
            .Take(topN)
            .ToList();

        var updatedOpps = allOpportunities
            .Where(o => o.UpdatedAtUtc.HasValue && o.UpdatedAtUtc > cutoff)
            .OrderByDescending(o => o.UpdatedAtUtc)
            .Take(topN)
            .ToList();

        var scoreUpEvents = (await _eventRepo.GetEventsSinceAsync(cutoff))
            .Where(e => e.EventType == OpportunityEventType.ScoreUp)
            .ToList();

        var newlyRelevantNoticeIds = scoreUpEvents.Select(e => e.NoticeId).Distinct().ToList();
        var newlyRelevantOpps = allOpportunities
            .Where(o => newlyRelevantNoticeIds.Contains(o.NoticeId))
            .OrderByDescending(o => scoreUpEvents.First(e => e.NoticeId == o.NoticeId).OccurredAtUtc)
            .Take(topN)
            .ToList();

        var digest = new DigestData
        {
            GeneratedAt = DateTime.UtcNow,
            CutoffTime = cutoff,
            NewOpportunities = await BuildDigestItemsAsync(newOpps),
            UpdatedOpportunities = await BuildDigestItemsAsync(updatedOpps),
            NewlyRelevantOpportunities = await BuildDigestItemsAsync(newlyRelevantOpps, scoreUpEvents)
        };

        return digest;
    }

    public async Task<int> SaveDigestAsync(DigestData digest, DigestDeliveryMethod method)
    {
        var profileVersion = await _versionRepo.GetLatestAsync();

        var digestEntity = new Digest
        {
            GeneratedAtUtc = digest.GeneratedAt,
            ProfileVersionId = profileVersion?.Id,
            DigestJson = JsonSerializer.Serialize(digest),
            SentVia = method,
            LastDigestCutoffUtc = digest.GeneratedAt
        };

        return await _digestRepo.InsertAsync(digestEntity);
    }

    private async Task<List<DigestItem>> BuildDigestItemsAsync(
        List<Opportunity> opportunities,
        List<OpportunityEvent>? events = null)
    {
        var items = new List<DigestItem>();

        foreach (var opp in opportunities)
        {
            var match = await _matchResultRepo.GetByNoticeIdAsync(opp.NoticeId);
            var scoreEvent = events?.FirstOrDefault(e => e.NoticeId == opp.NoticeId);

            var whyMatched = "";
            if (match != null && !string.IsNullOrWhiteSpace(match.ScoreBreakdownJson))
            {
                try
                {
                    var breakdown = JsonSerializer.Deserialize<ScoreBreakdown>(match.ScoreBreakdownJson);
                    var topFactors = breakdown?.Factors
                        .Where(f => f.Points > 0)
                        .OrderByDescending(f => f.Points)
                        .Take(3)
                        .Select(f => $"{f.Name} ({f.Points}pts)")
                        .ToList();

                    whyMatched = topFactors != null && topFactors.Any()
                        ? string.Join(", ", topFactors)
                        : "Low match";
                }
                catch
                {
                    whyMatched = "Score breakdown unavailable";
                }
            }

            items.Add(new DigestItem
            {
                NoticeId = opp.NoticeId,
                Title = opp.Title,
                Agency = opp.AgencyPathName,
                PostedDate = opp.PostedDate,
                ResponseDeadline = opp.ResponseDeadline,
                Score = match?.Score ?? 0,
                ScoreDelta = scoreEvent != null ? (scoreEvent.NewScore - scoreEvent.OldScore) : null,
                WhyMatchedSummary = whyMatched,
                UiUrl = opp.UiUrl
            });
        }

        return items;
    }
}
