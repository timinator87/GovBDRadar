using System.Windows;
using GovMatch.App.Commands;
using GovMatch.App.Services;
using GovMatch.Data.Repositories;

namespace GovMatch.App.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly AppSettingsRepository _settingsRepo;
    private string? _apiKey;
    private string _environment = "Production";
    private int _dailyRateLimitTier = 10;
    private bool _enablePrefetch;
    private int _prefetchThreshold = 75;

    public string? ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public string Environment
    {
        get => _environment;
        set => SetProperty(ref _environment, value);
    }

    public int DailyRateLimitTier
    {
        get => _dailyRateLimitTier;
        set => SetProperty(ref _dailyRateLimitTier, value);
    }

    public bool EnablePrefetch
    {
        get => _enablePrefetch;
        set => SetProperty(ref _enablePrefetch, value);
    }

    public int PrefetchThreshold
    {
        get => _prefetchThreshold;
        set => SetProperty(ref _prefetchThreshold, value);
    }

    public RelayCommand SaveCommand { get; }

    public SettingsViewModel(AppSettingsRepository settingsRepo)
    {
        _settingsRepo = settingsRepo;

        SaveCommand = new RelayCommand(async () => await SaveSettingsAsync());

        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsRepo.GetSettingsAsync();
            Environment = settings.Environment;
            DailyRateLimitTier = settings.DailyRateLimitTier;
            EnablePrefetch = settings.EnablePrefetch;
            PrefetchThreshold = settings.PrefetchThreshold;

            if (!string.IsNullOrWhiteSpace(settings.EncryptedApiKey))
            {
                ApiKey = "****** (Hidden)";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            var settings = await _settingsRepo.GetSettingsAsync();

            if (!string.IsNullOrWhiteSpace(ApiKey) && ApiKey != "****** (Hidden)")
            {
                settings.EncryptedApiKey = SecureStorageService.EncryptString(ApiKey);
            }

            settings.Environment = Environment;
            settings.DailyRateLimitTier = DailyRateLimitTier;
            settings.EnablePrefetch = EnablePrefetch;
            settings.PrefetchThreshold = PrefetchThreshold;

            await _settingsRepo.UpdateSettingsAsync(settings);

            MessageBox.Show("Settings saved successfully! Please restart the application for changes to take effect.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
