namespace Multitool.Core;

/// <summary>
/// Контракт для извлечения аудио из видео
/// </summary>
public interface IAudioExtractor
{
    /// <summary>
    /// Извлечение аудио из видеофайла
    /// </summary>
    /// <param name="videoPath">Путь к видеофайлу</param>
    /// <param name="outputPath">Путь для сохранения аудио</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Результат операции</returns>
    Task<ToolResult> ExtractAsync(string videoPath, string outputPath, CancellationToken ct = default);
}
