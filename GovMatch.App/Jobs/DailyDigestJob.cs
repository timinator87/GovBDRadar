using GovMatch.Core.Models;
using GovMatch.Data.Services;
using GovMatch.Data.Repositories;
using Quartz;

namespace GovMatch.App.Jobs;

[DisallowConcurrentExecution]
public class DailyDigestJob : IJob
{
    private readonly DigestService _digestService;
    private readonly AppSettingsRepository _settingsRepo;

    public DailyDigestJob(
        DigestService digestService,
        AppSettingsRepository settingsRepo)
    {
        _digestService = digestService;
        _settingsRepo = settingsRepo;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"[{DateTime.Now}] Running daily digest job...");

        try
        {
            var settings = await _settingsRepo.GetSettingsAsync();

            var digest = await _digestService.GenerateDigestAsync(topN: 20);

            var totalItems = digest.NewOpportunities.Count +
                            digest.UpdatedOpportunities.Count +
                            digest.NewlyRelevantOpportunities.Count;

            if (totalItems == 0)
            {
                Console.WriteLine("No new items for digest");
                await _digestService.SaveDigestAsync(digest, DigestDeliveryMethod.None);
                return;
            }

            var deliveryMethod = settings.EnableNotifications
                ? DigestDeliveryMethod.Toast
                : DigestDeliveryMethod.None;

            await _digestService.SaveDigestAsync(digest, deliveryMethod);

            Console.WriteLine($"Digest generated: {digest.NewOpportunities.Count} new, " +
                            $"{digest.UpdatedOpportunities.Count} updated, " +
                            $"{digest.NewlyRelevantOpportunities.Count} newly relevant");

            if (deliveryMethod == DigestDeliveryMethod.Toast)
            {
                Console.WriteLine($"TODO: Send toast notification for {totalItems} items");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Digest job failed: {ex.Message}");
        }
    }
}
