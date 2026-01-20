using GovMatch.Core.Interfaces;
using GovMatch.Core.Models;
using GovMatch.Core.Services;
using GovMatch.Data.Repositories;
using Quartz;

namespace GovMatch.App.Jobs;

[DisallowConcurrentExecution]
public class RescoreJob : IJob
{
    private readonly OpportunityRepository _opportunityRepo;
    private readonly MatchResultRepository _matchResultRepo;
    private readonly OpportunityEventRepository _eventRepo;
    private readonly ProfileVersioningService _versioningService;
    private readonly IScoringEngine _scoringEngine;

    public RescoreJob(
        OpportunityRepository opportunityRepo,
        MatchResultRepository matchResultRepo,
        OpportunityEventRepository eventRepo,
        ProfileVersioningService versioningService,
        IScoringEngine scoringEngine)
    {
        _opportunityRepo = opportunityRepo;
        _matchResultRepo = matchResultRepo;
        _eventRepo = eventRepo;
        _versioningService = versioningService;
        _scoringEngine = scoringEngine;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"[{DateTime.Now}] Starting re-score job...");

        var profileVersionId = context.MergedJobDataMap.GetInt("ProfileVersionId");

        var profileVersion = await _versioningService.GetCurrentVersionAsync();
        if (profileVersion == null || profileVersion.Id != profileVersionId)
        {
            Console.WriteLine($"Profile version {profileVersionId} not found or not current");
            return;
        }

        var profile = _versioningService.SnapshotToProfile(profileVersion);

        var allOpportunities = (await _opportunityRepo.GetAllAsync()).ToList();
        var activeOpportunities = allOpportunities
            .Where(o => o.Active || (o.ResponseDeadline.HasValue && o.ResponseDeadline >= DateTime.UtcNow.AddDays(-14)))
            .ToList();

        Console.WriteLine($"Re-scoring {activeOpportunities.Count} active opportunities...");

        var rescored = 0;
        var scoreUpEvents = 0;

        foreach (var opp in activeOpportunities)
        {
            var previousMatch = await _matchResultRepo.GetByNoticeIdAsync(opp.NoticeId);
            var oldScore = previousMatch?.Score ?? 0;

            var newMatch = _scoringEngine.ScoreOpportunity(profile, opp, opp.DescriptionText);
            newMatch.Id = await _matchResultRepo.InsertAsync(newMatch);

            rescored++;

            if (oldScore < 70 && newMatch.Score >= 70)
            {
                await _eventRepo.InsertAsync(new OpportunityEvent
                {
                    NoticeId = opp.NoticeId,
                    EventType = OpportunityEventType.ScoreUp,
                    OldScore = oldScore,
                    NewScore = newMatch.Score,
                    OccurredAtUtc = DateTime.UtcNow,
                    ProfileVersionId = profileVersionId
                });
                scoreUpEvents++;
            }
            else if (oldScore >= 70 && newMatch.Score < 70)
            {
                await _eventRepo.InsertAsync(new OpportunityEvent
                {
                    NoticeId = opp.NoticeId,
                    EventType = OpportunityEventType.ScoreDown,
                    OldScore = oldScore,
                    NewScore = newMatch.Score,
                    OccurredAtUtc = DateTime.UtcNow,
                    ProfileVersionId = profileVersionId
                });
            }
        }

        Console.WriteLine($"Re-score completed: {rescored} opportunities, {scoreUpEvents} newly relevant");
    }
}
