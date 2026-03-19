using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Multitool.UI.Models;
using Multitool.UI.Services;
using Multitool.UI.Views;

namespace Multitool.UI.ViewModels;

/// <summary>
/// ViewModel для главного меню
/// </summary>
public partial class MainMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Конструктор
    /// </summary>
    public MainMenuViewModel(INavigationService navigationService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        Tools = new ObservableCollection<ToolDescriptor>
        {
            new ToolDescriptor
            {
                Id = "audio-extractor",
                Name = "Извлечение аудио",
                Description = "Извлечь аудио из видеофайла",
                Icon = "🎵",
                PageType = typeof(Views.AudioExtractorPage)
            },
            new ToolDescriptor
            {
                Id = "hh-aggregator",
                Name = "Вакансии hh.ru",
                Description = "Агрегация вакансий с hh.ru в JSON",
                Icon = "📋",
                PageType = typeof(Views.HHVacanciesPage)
            },
            // Здесь можно добавить другие инструменты
        };
    }

    /// <summary>
    /// Список доступных инструментов
    /// </summary>
    public ObservableCollection<ToolDescriptor> Tools { get; }

    /// <summary>
    /// Команда выбора инструмента
    /// </summary>
    [RelayCommand]
    private void SelectTool(ToolDescriptor tool)
    {
        if (tool == null) return;

        // Создание страницы через DI для внедрения зависимостей
        var page = (System.Windows.Controls.Page)_serviceProvider.GetRequiredService(tool.PageType);
        _navigationService.NavigateToPage(page);
    }
}
