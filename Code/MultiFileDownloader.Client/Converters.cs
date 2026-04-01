using System;
using System.Globalization;
using System.Windows; // PHẢI CÓ DÒNG NÀY để dùng Visibility
using System.Windows.Data;
using System.Windows.Media;

namespace MultiFileDownloader.Client
{
    // 1. Chuyển bool IsPaused -> Chữ hiển thị trên nút
    public class PauseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPaused) return isPaused ? "Resume" : "Pause";
            return "Pause";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 2. Chuyển bool IsCompleted -> Màu sắc (Xanh dương sang Xanh lá)
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCompleted && isCompleted) return new SolidColorBrush(Colors.Green);
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D7CF6"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 3. Chuyển bool IsCompleted -> Trạng thái Ẩn/Hiện (Ẩn nút khi xong)
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCompleted)
                return isCompleted ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
}