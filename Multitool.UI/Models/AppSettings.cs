namespace Multitool.UI.Models;

/// <summary>
/// Настройки приложения с возможностью управления видимостью инструментов
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Настройки видимости для каждого инструмента по его ID
    /// </summary>
    public Dictionary<string, bool> ToolVisibility { get; set; } = new();
}
