using System.Windows.Controls;
using Multitool.UI.ViewModels;

namespace Multitool.UI.Views;

/// <summary>
/// Страница агрегации вакансий hh.ru
/// </summary>
public partial class HHVacanciesPage : Page
{
    public HHVacanciesPage(HHVacanciesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public HHVacanciesPage()
    {
        InitializeComponent();
    }
}
