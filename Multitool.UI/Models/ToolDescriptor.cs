using System.Windows.Controls;

namespace Multitool.UI.Models;

/// <summary>
/// Описание инструмента для отображения в меню
/// </summary>
public class ToolDescriptor
{
    /// <summary>
    /// Уникальный идентификатор инструмента
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Название инструмента
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Описание инструмента
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Иконка инструмента (символ или путь к изображению)
    /// </summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>
    /// Тип страницы для навигации
    /// </summary>
    public Type PageType { get; init; } = typeof(Page);
}
