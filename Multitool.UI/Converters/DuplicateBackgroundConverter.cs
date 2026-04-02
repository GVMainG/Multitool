// AI-Qwen2.5-Coder-32B-Instruct | 02-04-2026

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Multitool.UI.Converters;

/// <summary>
/// Конвертер для цвета фона токена в зависимости от статуса дубликата.
/// </summary>
public class DuplicateBackgroundConverter : IValueConverter
{
    /// <summary>
    /// Преобразование bool в SolidColorBrush для фона токена
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDuplicate)
        {
            return isDuplicate 
                ? new SolidColorBrush(Color.FromArgb(255, 255, 200, 200)) // Бледно-красный для дубликатов
                : new SolidColorBrush(Color.FromArgb(255, 227, 242, 253)); // Бледно-голубой для обычных
        }
        return new SolidColorBrush(Color.FromArgb(255, 227, 242, 253));
    }

    /// <summary>
    /// Обратное преобразование не поддерживается
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
