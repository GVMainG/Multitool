// AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026 | Добавлен using для поддержки CollectionChanged.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    private HashSet<int> _duplicateIds = new();

    public HHVacanciesViewModel(IHHService hhService, INavigationService navigationService)
    {
        _hhService = hhService;
        _navigationService = navigationService;

        // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
        // Подписка на изменение коллекции для авто-обновления кнопок.
        _tokens.CollectionChanged += (s, e) =>
        {
            LoadVacanciesCommand.NotifyCanExecuteChanged();
            AnalyzeDuplicates();
        };
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Список токенов (ID вакансий)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TokenViewModel> _tokens = new();

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Текст для ввода нового токена
    /// </summary>
    [ObservableProperty]
    private string _inputText = string.Empty;

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

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Множество ID-дубликатов для подсветки.
    /// </summary>
    public HashSet<int> DuplicateIds
    {
        get => _duplicateIds;
        set
        {
            if (_duplicateIds != value)
            {
                _duplicateIds = value;
                OnPropertyChanged(nameof(DuplicateIds));
                UpdateTokenDuplicates();
            }
        }
    }

    /// <summary>
    /// Команда возврата назад
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoHome();
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Команда добавления токена.
    /// </summary>
    [RelayCommand]
    private void AddToken()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        var parts = InputText.Split(new[] { ',', ';', ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var id = ExtractId(trimmed);
            if (id.HasValue)
            {
                var isDuplicate = Tokens.Any(t => t.Id == id.Value) || DuplicateIds.Contains(id.Value);
                Tokens.Add(new TokenViewModel
                {
                    Text = trimmed,
                    Id = id.Value,
                    IsDuplicate = isDuplicate
                });
                
                if (isDuplicate)
                    DuplicateIds.Add(id.Value);
            }
        }

        InputText = string.Empty;
        AnalyzeDuplicates();
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Команда удаления токена.
    /// </summary>
    [RelayCommand]
    private void RemoveToken(TokenViewModel token)
    {
        if (token != null)
        {
            Tokens.Remove(token);
            AnalyzeDuplicates();
        }
    }

        // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Команда очистки всех токенов.
    /// </summary>
    [RelayCommand]
    private void ClearTokens()
    {
        Tokens.Clear();
        DuplicateIds.Clear();
        StatusMessage = "Список очищен";
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Обработка нажатия клавиш в поле ввода.
    /// </summary>
    [RelayCommand]
    private void HandleKeyPress(KeyEventArgs e)
    {
        if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            e.Handled = true;
            AddTokenCommand.Execute(null);
        }
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Обновление статуса дубликатов для всех токенов.
    /// </summary>
    private void UpdateTokenDuplicates()
    {
        foreach (var token in Tokens)
        {
            token.IsDuplicate = DuplicateIds.Contains(token.Id);
        }
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Команда загрузки вакансий
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteLoad))]
    private async Task LoadVacanciesAsync(CancellationToken cancellationToken)
    {
        // Получаем уникальные ID из токенов.
        var ids = Tokens.Select(t => t.Id).Distinct().ToList();
        
        if (!ids.Any())
        {
            StatusMessage = "Введите ID вакансий через пробел или запятую";
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
        // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
        // Проверка доступности загрузки: не загружается и есть токены.
        return CanLoad && Tokens.Count > 0;
    }

    partial void OnCanLoadChanged(bool value)
    {
        LoadVacanciesCommand.NotifyCanExecuteChanged();
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Анализ дубликатов ID во входной строке и токенах.
    /// </summary>
    private void AnalyzeDuplicates()
    {
        var allIds = new List<int>();

        // Добавляем ID из существующих токенов
        allIds.AddRange(Tokens.Select(t => t.Id));

        // Добавляем ID из текущего ввода
        if (!string.IsNullOrWhiteSpace(InputText))
        {
            allIds.AddRange(ParseVacancyIdsWithDuplicates(InputText));
        }

        var seen = new HashSet<int>();
        var duplicates = new HashSet<int>();

        foreach (var id in allIds)
        {
            if (!seen.Add(id))
            {
                duplicates.Add(id);
            }
        }

        DuplicateIds = duplicates;
    }

    // AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
    /// <summary>
    /// Извлечение ID из текста (URL или число).
    /// </summary>
    private int? ExtractId(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Contains("hh.ru/vacancy/"))
        {
            return ExtractIdFromUrl(trimmed);
        }
        return int.TryParse(trimmed, out var id) ? id : null;
    }

    /// <summary>
    /// Парсинг ID вакансий из строки (возвращает все ID включая дубликаты).
    /// </summary>
    private List<int> ParseVacancyIdsWithDuplicates(string input)
    {
        var ids = new List<int>();
        var parts = input.Split(new[] { ',', ';', ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
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
    /// Парсинг ID вакансий из строки (без дубликатов)
    /// </summary>
    private List<int> ParseVacancyIds(string input)
    {
        var ids = ParseVacancyIdsWithDuplicates(input);
        return ids.Distinct().ToList();
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

// AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026
/// <summary>
/// ViewModel для токена (ID вакансии)
/// </summary>
public partial class TokenViewModel : ObservableObject
{
    /// <summary>
    /// Текст токена
    /// </summary>
    [ObservableProperty]
    private string _text = string.Empty;

    /// <summary>
    /// ID вакансии
    /// </summary>
    [ObservableProperty]
    private int _id;

    /// <summary>
    /// Является ли дубликатом
    /// </summary>
    [ObservableProperty]
    private bool _isDuplicate;
}
