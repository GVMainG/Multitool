using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Multitool.Core;
using Multitool.Infrastructure;
using Multitool.UI.Services;
using Multitool.UI.ViewModels;
using Multitool.UI.Views;

namespace Multitool.UI;

/// <summary>
/// Код приложения с настройкой DI контейнера
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Регистрация Infrastructure
                services.AddInfrastructure();

                // Регистрация сервисов
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<ISettingsService, SettingsService>();

                // Регистрация ViewModel
                services.AddSingleton<MainMenuViewModel>();
                services.AddTransient<AudioExtractorViewModel>();
                services.AddTransient<HHVacanciesViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Регистрация страниц
                services.AddTransient<MainMenuPage>();
                services.AddTransient<AudioExtractorPage>();
                services.AddTransient<HHVacanciesPage>();
                services.AddTransient<SettingsPage>();

                // Регистрация MainWindow
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    /// <summary>
    /// Получение сервиса из DI контейнера
    /// </summary>
    public T GetService<T>() where T : class
    {
        return _host.Services.GetRequiredService<T>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Инициализация навигации
        var navigationService = GetService<INavigationService>();
        var mainWindow = GetService<MainWindow>();
        var serviceProvider = _host.Services;

        navigationService.Initialize(mainWindow.GetFrame());
        mainWindow.Show();

        // Навигация на главную страницу меню
        var mainMenuPage = new MainMenuPage(_host.Services);
        navigationService.NavigateToPage(mainMenuPage);

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
