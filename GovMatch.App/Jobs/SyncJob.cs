using GovMatch.Integrations.SamGov.Services;
using Quartz;

namespace GovMatch.App;

public class SyncJob : IJob
{
    private readonly SyncService _syncService;

    public SyncJob(SyncService syncService)
    {
        _syncService = syncService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"[{DateTime.Now}] Running scheduled sync job...");

        try
        {
            var result = await _syncService.RunSyncAsync();
            Console.WriteLine($"Sync completed: {result.NewOpportunities} new opportunities");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sync failed: {ex.Message}");
        }
    }
}
