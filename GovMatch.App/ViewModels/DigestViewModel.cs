using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using GovMatch.App.Commands;
using GovMatch.Core.Models;
using GovMatch.Core.Services;
using GovMatch.Data.Repositories;

namespace GovMatch.App.ViewModels;

public class DigestViewModel : ViewModelBase
{
    private readonly DigestService _digestService;
    private readonly DigestRepository _digestRepo;
    private DigestData? _currentDigest;
    private ObservableCollection<Digest> _digestHistory = new();
    private Digest? _selectedHistoryItem;

    public DigestData? CurrentDigest
    {
        get => _currentDigest;
        set => SetProperty(ref _currentDigest, value);
    }

    public ObservableCollection<Digest> DigestHistory
    {
        get => _digestHistory;
        set => SetProperty(ref _digestHistory, value);
    }

    public Digest? SelectedHistoryItem
    {
        get => _selectedHistoryItem;
        set
        {
            if (SetProperty(ref _selectedHistoryItem, value) && value != null)
            {
                LoadHistoricalDigest(value);
            }
        }
    }

    public RelayCommand GenerateDigestCommand { get; }
    public RelayCommand RefreshHistoryCommand { get; }

    public DigestViewModel(DigestService digestService, DigestRepository digestRepo)
    {
        _digestService = digestService;
        _digestRepo = digestRepo;

        GenerateDigestCommand = new RelayCommand(async () => await GenerateDigestAsync());
        RefreshHistoryCommand = new RelayCommand(async () => await LoadHistoryAsync());

        _ = LoadLatestDigestAsync();
        _ = LoadHistoryAsync();
    }

    private async Task LoadLatestDigestAsync()
    {
        try
        {
            var latest = await _digestRepo.GetLatestAsync();
            if (latest != null && !string.IsNullOrWhiteSpace(latest.DigestJson))
            {
                CurrentDigest = JsonSerializer.Deserialize<DigestData>(latest.DigestJson);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading digest: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            var history = await _digestRepo.GetRecentAsync(10);
            DigestHistory = new ObservableCollection<Digest>(history);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task GenerateDigestAsync()
    {
        try
        {
            var digest = await _digestService.GenerateDigestAsync(topN: 20);
            await _digestService.SaveDigestAsync(digest, DigestDeliveryMethod.None);

            CurrentDigest = digest;

            await LoadHistoryAsync();

            MessageBox.Show("Digest generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating digest: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadHistoricalDigest(Digest digest)
    {
        if (!string.IsNullOrWhiteSpace(digest.DigestJson))
        {
            try
            {
                CurrentDigest = JsonSerializer.Deserialize<DigestData>(digest.DigestJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading digest: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
