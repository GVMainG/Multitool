using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Multitool.UI.ViewModels;

namespace Multitool.UI.Views;

/// <summary>
/// Страница настроек приложения
/// </summary>
public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    public SettingsPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        var viewModel = serviceProvider.GetRequiredService<SettingsViewModel>();
        DataContext = viewModel;
    }
}
