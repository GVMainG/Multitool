using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Multitool.Core;
using Multitool.UI.Services;

namespace Multitool.UI.ViewModels;

/// <summary>
/// ViewModel для страницы агрегации вакансий hh.ru
/// </summary>
public partial class HHVacanciesViewModel : ObservableObject
{
    private readonly IHHService _hhService;
    private readonly INavigationService _navigationService;
    private List<VacancyResult> _loadedResults = new();

    public HHVacanciesViewModel(IHHService hhService, INavigationService navigationService)
    {
        _hhService = hhService;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Список ID вакансий для загрузки (ввод пользователя)
    /// </summary>
    [ObservableProperty]
    private string _vacancyIdsInput = string.Empty;

    /// <summary>
    /// Статус операции
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Готов к загрузке вакансий";

    /// <summary>
    /// Доступность операции загрузки
    /// </summary>
    [ObservableProperty]
    private bool _canLoad = true;

    /// <summary>
    /// Количество успешно загруженных вакансий
    /// </summary>
    [ObservableProperty]
    private int _successCount;

    /// <summary>
    /// Команда возврата назад
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoHome();
    }

    /// <summary>
    /// Команда загрузки вакансий
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteLoad))]
    private async Task LoadVacanciesAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(VacancyIdsInput))
        {
            StatusMessage = "Введите ID вакансий через запятую";
            return;
        }

        // Парсинг ID
        var ids = ParseVacancyIds(VacancyIdsInput);
        if (!ids.Any())
        {
            StatusMessage = "Не удалось найти корректные ID вакансий";
            return;
        }

        CanLoad = false;
        StatusMessage = $"Загрузка {ids.Count} вакансий...";

        _loadedResults = new List<VacancyResult>();

        try
        {
            foreach (var id in ids)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var result = await _hhService.GetVacancyAsync(id, cancellationToken);
                _loadedResults.Add(result);
            }

            var successCount = _loadedResults.Count(r => r.Success);
            SuccessCount = successCount;
            StatusMessage = $"Загружено: {successCount} из {ids.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            CanLoad = true;
        }
    }

    private bool CanExecuteLoad()
    {
        return CanLoad && !string.IsNullOrWhiteSpace(VacancyIdsInput);
    }

    partial void OnVacancyIdsInputChanged(string value)
    {
        LoadVacanciesCommand.NotifyCanExecuteChanged();
    }

    partial void OnCanLoadChanged(bool value)
    {
        LoadVacanciesCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Парсинг ID вакансий из строки
    /// </summary>
    private List<int> ParseVacancyIds(string input)
    {
        var ids = new List<int>();
        var parts = input.Split(new[] { ',', ';', ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            // Извлечение ID из URL если вставлена ссылка
            var trimmed = part.Trim();
            if (trimmed.Contains("hh.ru/vacancy/"))
            {
                var id = ExtractIdFromUrl(trimmed);
                if (id.HasValue)
                    ids.Add(id.Value);
            }
            else if (int.TryParse(trimmed, out var id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    /// <summary>
    /// Извлечение ID из URL hh.ru
    /// </summary>
    private int? ExtractIdFromUrl(string url)
    {
        try
        {
            var startIndex = url.IndexOf("hh.ru/vacancy/") + 14;
            var endIndex = url.IndexOfAny(new[] { '?', '/', '&' }, startIndex);
            var idStr = endIndex > startIndex ? url.Substring(startIndex, endIndex - startIndex) : url.Substring(startIndex);
            return int.TryParse(idStr, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Команда сохранения в JSON
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteSaveJson))]
    private void SaveJson()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить вакансии в JSON",
            Filter = "JSON файлы|*.json|Все файлы|*.*",
            DefaultExt = "json",
            FileName = $"vacancies_{DateTime.Now:yyyy-MM-dd_HH-mm}.json"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            // Подсчёт статистики навыков
            var skillsCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var result in _loadedResults)
            {
                if (result.Success && result.Vacancy?.KeySkills != null)
                {
                    foreach (var skill in result.Vacancy.KeySkills)
                    {
                        if (!string.IsNullOrEmpty(skill.Name))
                        {
                            var skillName = skill.Name.Trim();
                            if (skillsCount.ContainsKey(skillName))
                                skillsCount[skillName]++;
                            else
                                skillsCount[skillName] = 1;
                        }
                    }
                }
            }

            // Топ-10 навыков
            var topSkills = skillsCount
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .Select(kvp => new SkillStat { Name = kvp.Key, Count = kvp.Value })
                .ToList();

            // Создание результата экспорта
            var exportResult = new VacancyExportResult
            {
                Vacancies = _loadedResults.Where(r => r.Success && r.Vacancy != null).Select(r => r.Vacancy!).ToList(),
                Statistics = new ExportStatistics
                {
                    TotalVacancies = _loadedResults.Count,
                    TopSkills = topSkills
                }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(exportResult, options);
            File.WriteAllText(dialog.FileName, json, Encoding.UTF8);

            StatusMessage = $"Сохранено {exportResult.Vacancies.Count} вакансий";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
            MessageBox.Show(
                $"Не удалось сохранить файл:\n\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private bool CanExecuteSaveJson()
    {
        return SuccessCount > 0;
    }

    partial void OnSuccessCountChanged(int value)
    {
        SaveJsonCommand.NotifyCanExecuteChanged();
    }
}
