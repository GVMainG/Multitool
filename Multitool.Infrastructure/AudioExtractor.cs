using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.IO.Compression;
using Multitool.Core;

namespace Multitool.Infrastructure;

/// <summary>
/// Реализация извлечения аудио через FFmpeg с автоматической загрузкой
/// </summary>
public sealed class AudioExtractor : IAudioExtractor
{
    private readonly string _ffmpegDirectory;
    private readonly string _ffmpegPath;
    private readonly string _ffmpegZipPath;
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Конструктор
    /// </summary>
    public AudioExtractor()
    {
        // Папка для хранения FFmpeg в локальном appdata
        _ffmpegDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Multitool",
            "ffmpeg");

        _ffmpegPath = Path.Combine(_ffmpegDirectory, "ffmpeg.exe");
        _ffmpegZipPath = Path.Combine(_ffmpegDirectory, "ffmpeg.zip");
    }

    /// <summary>
    /// Извлечение аудио из видеофайла
    /// </summary>
    public async Task<ToolResult> ExtractAsync(string videoPath, string outputPath, CancellationToken ct = default)
    {
        try
        {
            // Валидация входных путей
            if (string.IsNullOrWhiteSpace(videoPath))
                return new ToolResult(false, "Путь к видеофайлу не указан");

            if (string.IsNullOrWhiteSpace(outputPath))
                return new ToolResult(false, "Путь для сохранения аудио не указан");

            if (!File.Exists(videoPath))
                return new ToolResult(false, $"Видеофайл не найден: {videoPath}");

            // Загрузка/проверка FFmpeg
            var ffmpegResult = await EnsureFFmpegAsync(ct);
            if (!ffmpegResult.Success)
                return ffmpegResult;

            // Создание директории для выходного файла если не существует
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // Настройка процесса FFmpeg
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{videoPath}\" -vn -acodec libmp3lame -q:a 2 \"{outputPath}\" -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };

            // Подписка на вывод для возможного логирования
            var errorOutput = new System.Text.StringBuilder();
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorOutput.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            // Ожидание завершения с поддержкой отмены
            await Task.Run(() =>
            {
                process.WaitForExit();
            }, ct);

            if (process.ExitCode == 0)
                return new ToolResult(true, "Аудио успешно извлечено", outputPath);

            var errorMessage = errorOutput.ToString();
            return string.IsNullOrEmpty(errorMessage)
                ? new ToolResult(false, $"FFmpeg вернул код ошибки: {process.ExitCode}")
                : new ToolResult(false, $"Ошибка FFmpeg: {errorMessage}");
        }
        catch (OperationCanceledException)
        {
            return new ToolResult(false, "Операция отменена пользователем");
        }
        catch (Exception ex)
        {
            return new ToolResult(false, $"Критическая ошибка: {ex.Message}");
        }
    }

    /// <summary>
    /// Проверка и загрузка FFmpeg
    /// </summary>
    private async Task<ToolResult> EnsureFFmpegAsync(CancellationToken ct)
    {
        if (File.Exists(_ffmpegPath))
            return new ToolResult(true, $"FFmpeg найден: {_ffmpegPath}");

        try
        {
            if (!Directory.Exists(_ffmpegDirectory))
                Directory.CreateDirectory(_ffmpegDirectory);

            // URL для загрузки FFmpeg Essentials
            var ffmpegUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

            // Загрузка ZIP архива
            using var response = await _httpClient.GetAsync(ffmpegUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            using var fileStream = new FileStream(_ffmpegZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream, ct);
            fileStream.Close();

            // Распаковка архива
            using var archive = ZipFile.OpenRead(_ffmpegZipPath);
            
            // Поиск ffmpeg.exe в архиве (обычно в папке bin)
            var ffmpegEntry = archive.Entries
                .FirstOrDefault(e => e.Name.Equals("ffmpeg.exe", StringComparison.OrdinalIgnoreCase));

            if (ffmpegEntry == null)
            {
                // Попытка найти в подпапке bin
                ffmpegEntry = archive.Entries
                    .FirstOrDefault(e => e.FullName.EndsWith("bin/ffmpeg.exe", StringComparison.OrdinalIgnoreCase) ||
                                         e.FullName.EndsWith("bin\\ffmpeg.exe", StringComparison.OrdinalIgnoreCase));
            }

            if (ffmpegEntry == null)
                return new ToolResult(false, "ffmpeg.exe не найден в архиве");

            // Извлечение ffmpeg.exe
            ffmpegEntry.ExtractToFile(_ffmpegPath, overwrite: true);

            // Удаление ZIP архива
            try
            {
                File.Delete(_ffmpegZipPath);
            }
            catch
            {
                // Игнорируем ошибку удаления архива
            }

            return new ToolResult(true, $"FFmpeg загружен и извлечён: {_ffmpegPath}");
        }
        catch (HttpRequestException ex)
        {
            return new ToolResult(false, $"Ошибка загрузки FFmpeg: {ex.Message}\n\nПроверьте подключение к интернету или установите FFmpeg вручную в PATH.");
        }
        catch (Exception ex)
        {
            return new ToolResult(false, $"Ошибка установки FFmpeg: {ex.Message}");
        }
    }
}
