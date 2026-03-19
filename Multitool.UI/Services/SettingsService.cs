using System.IO;
using Multitool.UI.Models;

namespace Multitool.UI.Services;

/// <summary>
/// Сервис для управления настройками приложения
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Получить текущие настройки
    /// </summary>
    AppSettings GetSettings();

    /// <summary>
    /// Сохранить настройки
    /// </summary>
    void SaveSettings(AppSettings settings);

    /// <summary>
    /// Получить видимость инструмента по его ID
    /// </summary>
    bool IsToolVisible(string toolId);

    /// <summary>
    /// Установить видимость инструмента
    /// </summary>
    void SetToolVisibility(string toolId, bool isVisible);
}

/// <summary>
/// Реализация сервиса настроек с сохранением в JSON файл
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private AppSettings _currentSettings;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDir = Path.Combine(appData, "Multitool");
        Directory.CreateDirectory(appDir);
        _settingsPath = Path.Combine(appDir, "settings.json");
        _currentSettings = LoadSettings();
    }

    public AppSettings GetSettings() => _currentSettings;

    public void SaveSettings(AppSettings settings)
    {
        _currentSettings = settings;
        var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_settingsPath, json);
    }

    public bool IsToolVisible(string toolId)
    {
        if (_currentSettings.ToolVisibility.TryGetValue(toolId, out var isVisible))
        {
            return isVisible;
        }
        // По умолчанию все инструменты видимы
        return true;
    }

    public void SetToolVisibility(string toolId, bool isVisible)
    {
        _currentSettings.ToolVisibility[toolId] = isVisible;
        SaveSettings(_currentSettings);
    }

    private AppSettings LoadSettings()
    {
        if (File.Exists(_settingsPath))
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                return System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }
        return new AppSettings();
    }
}
