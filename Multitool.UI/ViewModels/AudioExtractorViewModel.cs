using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Multitool.Core;
using Multitool.UI.Services;

namespace Multitool.UI.ViewModels;

/// <summary>
/// ViewModel для страницы извлечения аудио
/// </summary>
public partial class AudioExtractorViewModel : ObservableObject
{
    private readonly IAudioExtractor _audioExtractor;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Конструктор с внедрением зависимостей
    /// </summary>
    public AudioExtractorViewModel(IAudioExtractor audioExtractor, INavigationService navigationService)
    {
        _audioExtractor = audioExtractor;
        _navigationService = navigationService;
    }

    /// <summary>
    /// Путь к видеофайлу
    /// </summary>
    [ObservableProperty]
    private string _videoPath = string.Empty;

    /// <summary>
    /// Путь для сохранения аудио
    /// </summary>
    [ObservableProperty]
    private string _outputPath = string.Empty;

    /// <summary>
    /// Сообщение о статусе операции
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Готов к работе";

    /// <summary>
    /// Доступность операции извлечения
    /// </summary>
    [ObservableProperty]
    private bool _canExtract;

    /// <summary>
    /// Команда возврата назад
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _navigationService.GoHome();
    }

    /// <summary>
    /// Команда выбора видеофайла
    /// </summary>
    [RelayCommand]
    private void BrowseVideo()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите видеофайл",
            Filter = "Видеофайлы|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv|Все файлы|*.*",
            FilterIndex = 1
        };

        if (dialog.ShowDialog() == true)
        {
            VideoPath = dialog.FileName;

            // Автоматическая генерация пути для аудио
            var videoDir = Path.GetDirectoryName(VideoPath);
            var videoName = Path.GetFileNameWithoutExtension(VideoPath);
            OutputPath = Path.Combine(videoDir ?? string.Empty, $"{videoName}.mp3");

            StatusMessage = $"Выбран файл: {VideoPath}";
        }
    }

    partial void OnVideoPathChanged(string value)
    {
        UpdateCanExtract();
    }

    partial void OnOutputPathChanged(string value)
    {
        UpdateCanExtract();
    }

    /// <summary>
    /// Команда выбора пути сохранения
    /// </summary>
    [RelayCommand]
    private void BrowseOutput()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить аудио как",
            Filter = "MP3 файлы|*.mp3|Все файлы|*.*",
            FilterIndex = 1,
            DefaultExt = "mp3",
            AddExtension = true
        };

        if (!string.IsNullOrEmpty(VideoPath))
        {
            var videoName = Path.GetFileNameWithoutExtension(VideoPath);
            dialog.FileName = $"{videoName}.mp3";
        }

        if (dialog.ShowDialog() == true)
        {
            OutputPath = dialog.FileName;
            StatusMessage = $"Путь сохранения: {OutputPath}";
        }
    }

    /// <summary>
    /// Команда извлечения аудио
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteExtract))]
    private async Task ExtractAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(VideoPath) || string.IsNullOrEmpty(OutputPath))
        {
            StatusMessage = "Ошибка: Выберите видеофайл и путь сохранения";
            return;
        }

        CanExtract = false;
        StatusMessage = "Извлечение аудио...";

        try
        {
            var result = await _audioExtractor.ExtractAsync(VideoPath, OutputPath, cancellationToken);

            if (result.Success)
            {
                StatusMessage = $"Успешно! Файл сохранён: {result.OutputPath}";
                MessageBox.Show(
                    $"Аудио успешно извлечено!\n\nПуть: {result.OutputPath}",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = $"Ошибка: {result.Message}";
                MessageBox.Show(
                    $"Не удалось извлечь аудио:\n\n{result.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Критическая ошибка: {ex.Message}";
            MessageBox.Show(
                $"Произошла ошибка:\n\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            CanExtract = true;
        }
    }

    partial void OnCanExtractChanged(bool value)
    {
        ExtractCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Проверка доступности команды извлечения
    /// </summary>
    private bool CanExecuteExtract()
    {
        return CanExtract && !string.IsNullOrEmpty(VideoPath) && !string.IsNullOrEmpty(OutputPath);
    }

    /// <summary>
    /// Обновление флага доступности извлечения
    /// </summary>
    private void UpdateCanExtract()
    {
        CanExtract = !string.IsNullOrEmpty(VideoPath) && !string.IsNullOrEmpty(OutputPath);
    }
}
