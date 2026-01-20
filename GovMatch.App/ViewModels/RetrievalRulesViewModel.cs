using System.Collections.ObjectModel;
using System.Windows;
using GovMatch.App.Commands;
using GovMatch.Core.Models;
using GovMatch.Data.Repositories;

namespace GovMatch.App.ViewModels;

public class RetrievalRulesViewModel : ViewModelBase
{
    private readonly RetrievalRuleRepository _ruleRepo;
    private ObservableCollection<RetrievalRule> _rules = new();
    private RetrievalRule? _selectedRule;

    public ObservableCollection<RetrievalRule> Rules
    {
        get => _rules;
        set => SetProperty(ref _rules, value);
    }

    public RetrievalRule? SelectedRule
    {
        get => _selectedRule;
        set => SetProperty(ref _selectedRule, value);
    }

    public RelayCommand AddRuleCommand { get; }
    public RelayCommand SaveRuleCommand { get; }
    public RelayCommand DeleteRuleCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public RetrievalRulesViewModel(RetrievalRuleRepository ruleRepo)
    {
        _ruleRepo = ruleRepo;

        AddRuleCommand = new RelayCommand(AddNewRule);
        SaveRuleCommand = new RelayCommand(async () => await SaveRuleAsync(), () => SelectedRule != null);
        DeleteRuleCommand = new RelayCommand(async () => await DeleteRuleAsync(), () => SelectedRule != null);
        RefreshCommand = new RelayCommand(async () => await LoadRulesAsync());

        _ = LoadRulesAsync();
    }

    private async Task LoadRulesAsync()
    {
        try
        {
            var rules = await _ruleRepo.GetAllAsync();
            Rules = new ObservableCollection<RetrievalRule>(rules);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddNewRule()
    {
        var newRule = new RetrievalRule
        {
            Name = $"New Rule {Rules.Count + 1}",
            Enabled = true,
            DaysBack = 3,
            MaxPages = 10,
            RequestBudget = 50
        };

        Rules.Add(newRule);
        SelectedRule = newRule;
    }

    private async Task SaveRuleAsync()
    {
        if (SelectedRule == null) return;

        try
        {
            if (SelectedRule.Id == 0)
            {
                SelectedRule.Id = await _ruleRepo.InsertAsync(SelectedRule);
            }
            else
            {
                await _ruleRepo.UpdateAsync(SelectedRule);
            }

            MessageBox.Show("Rule saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadRulesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving rule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteRuleAsync()
    {
        if (SelectedRule == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete '{SelectedRule.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _ruleRepo.DeleteAsync(SelectedRule.Id);
                await LoadRulesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting rule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
