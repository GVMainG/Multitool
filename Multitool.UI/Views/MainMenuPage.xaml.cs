using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Multitool.UI.Services;
using Multitool.UI.ViewModels;

namespace Multitool.UI.Views;

/// <summary>
/// Страница главного меню с плитками инструментов
/// </summary>
public partial class MainMenuPage : Page
{
    private readonly IServiceProvider? _serviceProvider;

    public MainMenuPage()
    {
        InitializeComponent();
    }

    public MainMenuPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        DataContext = serviceProvider.GetRequiredService<MainMenuViewModel>();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null) return;

        var settingsPage = new SettingsPage(_serviceProvider);

        var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
        navigationService.NavigateToPage(settingsPage);
    }
}
