using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Multitool.UI.Services;

/// <summary>
/// Интерфейс сервиса навигации
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Инициализация сервиса с Frame
    /// </summary>
    void Initialize(Frame frame);

    /// <summary>
    /// Навигация к странице
    /// </summary>
    void NavigateTo<TPage>() where TPage : Page, new();

    /// <summary>
    /// Навигация к странице с параметром
    /// </summary>
    void NavigateTo<TPage>(object parameter) where TPage : Page, new();

    /// <summary>
    /// Навигация к экземпляру страницы
    /// </summary>
    void NavigateToPage(Page page);

    /// <summary>
    /// Возврат к предыдущей странице
    /// </summary>
    void GoBack();

    /// <summary>
    /// Переход на главную страницу
    /// </summary>
    void GoHome();

    /// <summary>
    /// Доступность возврата назад
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Событие изменения состояния навигации
    /// </summary>
    event EventHandler CanGoBackChanged;
}

/// <summary>
/// Реализация сервиса навигации
/// </summary>
public class NavigationService : INavigationService
{
    private Frame? _frame;

    public event EventHandler? CanGoBackChanged;

    /// <summary>
    /// Инициализация сервиса с Frame
    /// </summary>
    public void Initialize(Frame frame)
    {
        _frame = frame;
        _frame.Navigated += OnNavigated;
    }

    private void OnNavigated(object? sender, NavigationEventArgs e)
    {
        CanGoBackChanged?.Invoke(this, EventArgs.Empty);
    }

    public void NavigateTo<TPage>() where TPage : Page, new()
    {
        _frame?.Navigate(new TPage());
    }

    public void NavigateTo<TPage>(object parameter) where TPage : Page, new()
    {
        _frame?.Navigate(new TPage { DataContext = parameter });
    }

    public void NavigateToPage(Page page)
    {
        _frame?.Navigate(page);
    }

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }

    public void GoHome()
    {
        while (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }

    public bool CanGoBack => _frame?.CanGoBack == true;
}
