using System.Collections.ObjectModel;
using System.Windows;
using GovMatch.App.Commands;
using GovMatch.Data.Services;
using GovMatch.Integrations.SamGov.Services;

namespace GovMatch.App.ViewModels;

public class SyncLogsViewModel : ViewModelBase
{
    private readonly SyncService _syncService;
    private readonly RateLimiter _rateLimiter;
    private ObservableCollection<string> _logs = new();
    private int _remainingRequests;
    private bool _isSyncing;

    public ObservableCollection<string> Logs
    {
        get => _logs;
        set => SetProperty(ref _logs, value);
    }

    public int RemainingRequests
    {
        get => _remainingRequests;
        set => SetProperty(ref _remainingRequests, value);
    }

    public bool IsSyncing
    {
        get => _isSyncing;
        set => SetProperty(ref _isSyncing, value);
    }

    public RelayCommand RunSyncCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public SyncLogsViewModel(SyncService syncService, RateLimiter rateLimiter)
    {
        _syncService = syncService;
        _rateLimiter = rateLimiter;

        RunSyncCommand = new RelayCommand(async () => await RunSyncAsync(), () => !IsSyncing);
        RefreshCommand = new RelayCommand(async () => await RefreshStatsAsync());

        _ = RefreshStatsAsync();
    }

    private async Task RunSyncAsync()
    {
        IsSyncing = true;
        Logs.Clear();
        Logs.Add($"[{DateTime.Now:HH:mm:ss}] Starting sync...");

        try
        {
            var result = await _syncService.RunSyncAsync();

            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Sync completed!");
            Logs.Add($"  - Opportunities fetched: {result.OpportunitiesFetched}");
            Logs.Add($"  - New opportunities: {result.NewOpportunities}");
            Logs.Add($"  - Requests made: {result.RequestsMade}");

            if (result.Errors.Any())
            {
                Logs.Add($"  - Errors: {result.Errors.Count}");
                foreach (var error in result.Errors)
                {
                    Logs.Add($"    * {error}");
                }
            }

            await RefreshStatsAsync();
        }
        catch (Exception ex)
        {
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}");
            MessageBox.Show($"Sync failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSyncing = false;
        }
    }

    private async Task RefreshStatsAsync()
    {
        try
        {
            RemainingRequests = await _rateLimiter.GetRemainingRequestsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error refreshing stats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
