using System.Windows;
using System.Windows.Controls;

namespace Multitool.UI;

/// <summary>
/// Главное окно приложения
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Получение Frame для инициализации навигации
    /// </summary>
    public Frame GetFrame() => MainFrame;
}
