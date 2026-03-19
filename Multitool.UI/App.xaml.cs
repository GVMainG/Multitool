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

                // Регистрация ViewModel
                services.AddTransient<MainMenuViewModel>();
                services.AddTransient<AudioExtractorViewModel>();
                services.AddTransient<HHVacanciesViewModel>();

                // Регистрация страниц
                services.AddTransient<MainMenuPage>();
                services.AddTransient<AudioExtractorPage>();
                services.AddTransient<HHVacanciesPage>();

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
        var mainMenuViewModel = GetService<MainMenuViewModel>();
        var mainMenuPage = new MainMenuPage(mainMenuViewModel, _host.Services);
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
