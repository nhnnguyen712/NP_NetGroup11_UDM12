using System;
using System.Windows;
using System.Threading.Tasks;

namespace MultiFileDownloader.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Bắt lỗi trên UI Thread (Các lỗi xảy ra trực tiếp trên giao diện)
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // 2. Bắt lỗi trên các luồng chạy ngầm (Toàn bộ App Domain)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // 3. Bắt lỗi riêng cho các Task/Async (Cực kỳ quan trọng cho ứng dụng Download)
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        // Xử lý lỗi từ giao diện người dùng
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError(e.Exception.Message);
            e.Handled = true; // Đánh dấu đã xử lý để App không bị đóng (Crash)
        }

        // Xử lý lỗi tổng thể hệ thống
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        // Xử lý lỗi phát sinh từ các Task tải file chạy ngầm
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            // Lấy lỗi chi tiết nhất từ bên trong Task
            var message = e.Exception.InnerException?.Message ?? e.Exception.Message;
            ShowError(message);
            
            // Đánh dấu đã quan sát lỗi để tránh rò rỉ bộ nhớ
            e.SetObserved(); 
        }

        // Hàm hiển thị thông báo lỗi an toàn cho mọi Thread
        private void ShowError(string message)
        {
            // Sử dụng Dispatcher để ép việc hiển thị MessageBox phải chạy trên UI Thread
            // Tránh lỗi "Calling thread cannot access this object"
            Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBox.Show(
                    $"Hệ thống gặp sự cố: {message}",
                    "Thông báo lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }));
        }
    }
}