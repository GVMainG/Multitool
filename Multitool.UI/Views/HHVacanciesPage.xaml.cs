using System.Windows.Controls;
using Multitool.UI.ViewModels;

namespace Multitool.UI.Views;

/// <summary>
/// Страница агрегации вакансий hh.ru
/// </summary>
// AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026 | Файл упрощён, логика перемещена в ViewModel
public partial class HHVacanciesPage : Page
{
    /// <summary>
    /// Конструктор страницы с внедрением ViewModel
    /// </summary>
    public HHVacanciesPage(HHVacanciesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>
    /// Конструктор по умолчанию для дизайнера
    /// </summary>
    public HHVacanciesPage()
    {
        InitializeComponent();
    }
}
