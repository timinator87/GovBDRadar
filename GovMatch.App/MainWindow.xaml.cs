using System.Windows;
using GovMatch.App.ViewModels;

namespace GovMatch.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
