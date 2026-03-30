using System;
using System.Windows;

namespace MultiFileDownloader.Client
{
    // Class App kế thừa từ Application (điểm bắt đầu của app WPF)
    public partial class App : Application
    {
        // Hàm chạy đầu tiên khi app khởi động
        protected override void OnStartup(StartupEventArgs e)
        {
            // Gọi hàm gốc của WPF
            base.OnStartup(e);

            // Gắn sự kiện bắt lỗi toàn cục (UI thread)
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        // Hàm xử lý khi có lỗi chưa được bắt trong UI
        private void App_DispatcherUnhandledException(
            object sender, 
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Hiển thị thông báo lỗi thân thiện cho người dùng
            // (thay vì lỗi crash mặc định của Windows)
            MessageBox.Show(
                $"Có lỗi xảy ra trong quá trình tải: {e.Exception.Message}", // Nội dung lỗi
                "Thông báo hệ thống", // Tiêu đề
                MessageBoxButton.OK, // Nút OK
                MessageBoxImage.Error // Icon lỗi
            );

            // Đánh dấu lỗi đã xử lý → app KHÔNG bị crash
            e.Handled = true;
        }
    }
}