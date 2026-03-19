using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Multitool.UI.ViewModels;

namespace Multitool.UI.Views;

/// <summary>
/// Страница главного меню с плитками инструментов
/// </summary>
public partial class MainMenuPage : Page
{
    public MainMenuPage(MainMenuViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public MainMenuPage(MainMenuViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public MainMenuPage()
    {
        InitializeComponent();
    }
}
