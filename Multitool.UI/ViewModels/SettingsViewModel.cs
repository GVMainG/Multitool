using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Multitool.UI.Models;
using Multitool.UI.Services;

namespace Multitool.UI.ViewModels;

/// <summary>
/// ViewModel для страницы настроек
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private readonly MainMenuViewModel _mainMenuViewModel;

    /// <summary>
    /// Конструктор
    /// </summary>
    public SettingsViewModel(
        ISettingsService settingsService,
        INavigationService navigationService,
        MainMenuViewModel mainMenuViewModel)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        _mainMenuViewModel = mainMenuViewModel;

        Tools = new ObservableCollection<ToolSetting>();
        LoadToolSettings();
    }

    /// <summary>
    /// Список настроек видимости инструментов
    /// </summary>
    public ObservableCollection<ToolSetting> Tools { get; }

    /// <summary>
    /// Загрузка настроек инструментов
    /// </summary>
    private void LoadToolSettings()
    {
        var allTools = new List<(string Id, string Name)>
        {
            ("audio-extractor", "Извлечение аудио"),
            ("hh-aggregator", "Вакансии hh.ru"),
        };

        Tools.Clear();
        foreach (var tool in allTools)
        {
            var isVisible = _settingsService.IsToolVisible(tool.Id);
            Tools.Add(new ToolSetting
            {
                Id = tool.Id,
                Name = tool.Name,
                IsVisible = isVisible
            });
        }
    }

    /// <summary>
    /// Команда сохранения настроек
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        foreach (var tool in Tools)
        {
            _settingsService.SetToolVisibility(tool.Id, tool.IsVisible);
        }

        // Обновить главное меню
        _mainMenuViewModel.RefreshTools();

        // Вернуться назад
        _navigationService.GoBack();
    }

    /// <summary>
    /// Команда отмены
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _navigationService.GoBack();
    }
}

/// <summary>
/// Модель настройки видимости инструмента
/// </summary>
public partial class ToolSetting : ObservableObject
{
    /// <summary>
    /// Идентификатор инструмента
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Название инструмента
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Видимость инструмента
    /// </summary>
    [ObservableProperty]
    private bool _isVisible;
}
