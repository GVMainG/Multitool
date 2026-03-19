using System.Windows.Controls;
using Multitool.UI.ViewModels;

namespace Multitool.UI.Views;

/// <summary>
/// Страница извлечения аудио
/// </summary>
public partial class AudioExtractorPage : Page
{
    public AudioExtractorPage(AudioExtractorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public AudioExtractorPage()
    {
        InitializeComponent();
    }
}
