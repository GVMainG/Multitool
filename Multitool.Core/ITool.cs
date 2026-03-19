namespace Multitool.Core;

/// <summary>
/// Контракт для всех инструментов мультитула
/// </summary>
public interface ITool
{
    /// <summary>
    /// Название инструмента
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Описание инструмента
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Выполнение инструмента
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат выполнения</returns>
    Task<ToolResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
