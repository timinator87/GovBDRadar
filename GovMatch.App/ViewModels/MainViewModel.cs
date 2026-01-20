using GovMatch.App.Commands;

namespace GovMatch.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public InboxViewModel InboxViewModel { get; }
    public CompanyProfileViewModel ProfileViewModel { get; }
    public RetrievalRulesViewModel RulesViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public SyncLogsViewModel SyncLogsViewModel { get; }

    public RelayCommand ShowInboxCommand { get; }
    public RelayCommand ShowProfileCommand { get; }
    public RelayCommand ShowRulesCommand { get; }
    public RelayCommand ShowSettingsCommand { get; }
    public RelayCommand ShowSyncLogsCommand { get; }

    public MainViewModel(
        InboxViewModel inboxViewModel,
        CompanyProfileViewModel profileViewModel,
        RetrievalRulesViewModel rulesViewModel,
        SettingsViewModel settingsViewModel,
        SyncLogsViewModel syncLogsViewModel)
    {
        InboxViewModel = inboxViewModel;
        ProfileViewModel = profileViewModel;
        RulesViewModel = rulesViewModel;
        SettingsViewModel = settingsViewModel;
        SyncLogsViewModel = syncLogsViewModel;

        ShowInboxCommand = new RelayCommand(() => CurrentView = InboxViewModel);
        ShowProfileCommand = new RelayCommand(() => CurrentView = ProfileViewModel);
        ShowRulesCommand = new RelayCommand(() => CurrentView = RulesViewModel);
        ShowSettingsCommand = new RelayCommand(() => CurrentView = SettingsViewModel);
        ShowSyncLogsCommand = new RelayCommand(() => CurrentView = SyncLogsViewModel);

        CurrentView = InboxViewModel;
    }
}
