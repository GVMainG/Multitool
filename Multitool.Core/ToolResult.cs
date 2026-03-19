namespace Multitool.Core;

/// <summary>
/// Результат выполнения инструмента
/// </summary>
/// <param name="Success">Успешность выполнения</param>
/// <param name="Message">Сообщение о результате</param>
/// <param name="OutputPath">Путь к выходному файлу (если применимо)</param>
public record ToolResult(bool Success, string Message, string? OutputPath = null);
