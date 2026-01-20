using System.Windows;
using GovMatch.App.Commands;
using GovMatch.Core.Models;
using GovMatch.Data.Repositories;

namespace GovMatch.App.ViewModels;

public class CompanyProfileViewModel : ViewModelBase
{
    private readonly CompanyProfileRepository _profileRepo;
    private CompanyProfile _profile;

    public string DisplayName
    {
        get => _profile.DisplayName;
        set
        {
            _profile.DisplayName = value;
            OnPropertyChanged();
        }
    }

    public string? CapabilityText
    {
        get => _profile.CapabilityText;
        set
        {
            _profile.CapabilityText = value;
            OnPropertyChanged();
        }
    }

    public string? KeywordsCsv
    {
        get => _profile.KeywordsCsv;
        set
        {
            _profile.KeywordsCsv = value;
            OnPropertyChanged();
        }
    }

    public string? NaicsCsv
    {
        get => _profile.NaicsCsv;
        set
        {
            _profile.NaicsCsv = value;
            OnPropertyChanged();
        }
    }

    public string? PscCsv
    {
        get => _profile.PscCsv;
        set
        {
            _profile.PscCsv = value;
            OnPropertyChanged();
        }
    }

    public string? TargetAgencyCodesCsv
    {
        get => _profile.TargetAgencyCodesCsv;
        set
        {
            _profile.TargetAgencyCodesCsv = value;
            OnPropertyChanged();
        }
    }

    public string? SetAsidePreferencesCsv
    {
        get => _profile.SetAsidePreferencesCsv;
        set
        {
            _profile.SetAsidePreferencesCsv = value;
            OnPropertyChanged();
        }
    }

    public string? PastPerformanceText
    {
        get => _profile.PastPerformanceText;
        set
        {
            _profile.PastPerformanceText = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }

    public CompanyProfileViewModel(CompanyProfileRepository profileRepo)
    {
        _profileRepo = profileRepo;
        _profile = new CompanyProfile { Id = 1 };

        SaveCommand = new RelayCommand(async () => await SaveProfileAsync());

        _ = LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            _profile = await _profileRepo.GetProfileAsync();
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(CapabilityText));
            OnPropertyChanged(nameof(KeywordsCsv));
            OnPropertyChanged(nameof(NaicsCsv));
            OnPropertyChanged(nameof(PscCsv));
            OnPropertyChanged(nameof(TargetAgencyCodesCsv));
            OnPropertyChanged(nameof(SetAsidePreferencesCsv));
            OnPropertyChanged(nameof(PastPerformanceText));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SaveProfileAsync()
    {
        try
        {
            await _profileRepo.UpdateProfileAsync(_profile);
            MessageBox.Show("Profile saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
