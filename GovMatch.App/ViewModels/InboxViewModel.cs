using System.Collections.ObjectModel;
using System.Windows;
using GovMatch.App.Commands;
using GovMatch.App.Services;
using GovMatch.Core.Models;
using GovMatch.Data.Repositories;

namespace GovMatch.App.ViewModels;

public class InboxViewModel : ViewModelBase
{
    private readonly OpportunityRepository _opportunityRepo;
    private readonly MatchResultRepository _matchResultRepo;
    private readonly UserActionRepository _userActionRepo;
    private readonly ExportService _exportService;

    private ObservableCollection<OpportunityListItem> _opportunities = new();
    private OpportunityListItem? _selectedOpportunity;
    private int _minScore = 0;
    private bool _activeOnly = true;
    private string? _searchText;
    private bool _isLoading;

    public ObservableCollection<OpportunityListItem> Opportunities
    {
        get => _opportunities;
        set => SetProperty(ref _opportunities, value);
    }

    public OpportunityListItem? SelectedOpportunity
    {
        get => _selectedOpportunity;
        set
        {
            if (SetProperty(ref _selectedOpportunity, value) && value != null)
            {
                LoadOpportunityDetailsAsync(value);
            }
        }
    }

    public int MinScore
    {
        get => _minScore;
        set
        {
            if (SetProperty(ref _minScore, value))
                _ = LoadOpportunitiesAsync();
        }
    }

    public bool ActiveOnly
    {
        get => _activeOnly;
        set
        {
            if (SetProperty(ref _activeOnly, value))
                _ = LoadOpportunitiesAsync();
        }
    }

    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ExportCommand { get; }

    public InboxViewModel(
        OpportunityRepository opportunityRepo,
        MatchResultRepository matchResultRepo,
        UserActionRepository userActionRepo,
        ExportService exportService)
    {
        _opportunityRepo = opportunityRepo;
        _matchResultRepo = matchResultRepo;
        _userActionRepo = userActionRepo;
        _exportService = exportService;

        SearchCommand = new RelayCommand(async () => await LoadOpportunitiesAsync());
        RefreshCommand = new RelayCommand(async () => await LoadOpportunitiesAsync());
        ExportCommand = new RelayCommand(async () => await ExportOpportunitiesAsync());

        _ = LoadOpportunitiesAsync();
    }

    private async Task LoadOpportunitiesAsync()
    {
        IsLoading = true;
        try
        {
            var criteria = new OpportunitySearchCriteria
            {
                MinScore = MinScore > 0 ? MinScore : null,
                ActiveOnly = ActiveOnly,
                SearchText = SearchText
            };

            var opps = await _opportunityRepo.SearchAsync(criteria);
            var items = new List<OpportunityListItem>();

            foreach (var opp in opps)
            {
                var match = await _matchResultRepo.GetByNoticeIdAsync(opp.NoticeId);
                var action = await _userActionRepo.GetByNoticeIdAsync(opp.NoticeId);

                items.Add(new OpportunityListItem
                {
                    Opportunity = opp,
                    Score = match?.Score ?? 0,
                    Status = action?.ActionType.ToString() ?? "None"
                });
            }

            Opportunities = new ObservableCollection<OpportunityListItem>(items.OrderByDescending(i => i.Score));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading opportunities: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async void LoadOpportunityDetailsAsync(OpportunityListItem item)
    {
        // This would trigger loading the detail view
        // For now, just placeholder
        await Task.CompletedTask;
    }

    private async Task ExportOpportunitiesAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var viewModels = Opportunities.Select(o => new OpportunityViewModel
                {
                    NoticeId = o.Opportunity.NoticeId,
                    Title = o.Opportunity.Title,
                    Score = o.Score,
                    Agency = o.Opportunity.AgencyPathName ?? "",
                    PostedDate = o.Opportunity.PostedDate,
                    ResponseDeadline = o.Opportunity.ResponseDeadline,
                    NaicsCode = o.Opportunity.NaicsCode,
                    ClassificationCode = o.Opportunity.ClassificationCode,
                    SetAside = o.Opportunity.SetAsideDescription,
                    Type = o.Opportunity.Type,
                    Status = o.Status,
                    UiUrl = o.Opportunity.UiUrl
                });

                await _exportService.ExportToCsvAsync(viewModels, dialog.FileName);
                MessageBox.Show("Export completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

public class OpportunityListItem
{
    public required Opportunity Opportunity { get; set; }
    public int Score { get; set; }
    public string Status { get; set; } = "";
}
