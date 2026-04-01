using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

namespace MultiFileDownloader.Client
{
    /// <summary>
    /// Logic xử lý chính cho MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Danh sách quản lý các item đang tải, tự động cập nhật lên ListView nhờ Binding
        public ObservableCollection<DownloadItem> Downloads { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Khởi tạo danh sách và gắn DataContext để XAML nhận diện được {Binding Downloads}
            Downloads = new ObservableCollection<DownloadItem>();
            this.DataContext = this;

            // Nạp dữ liệu giả lập cho danh sách file trên Server
            LoadServerFiles();

          
        }

        private void LoadServerFiles()
        {
            lstServerFiles.Items.Add("📄 Báo_cáo_cuối_kỳ.docx");
            lstServerFiles.Items.Add("🎬 Video_hướng_dẫn.mp4");
            lstServerFiles.Items.Add("🎵 Nhạc_nền_project.mp3");
            lstServerFiles.Items.Add("📦 Source_code_v1.zip");
            lstServerFiles.Items.Add("🖼️ Infographic_safety.png");
        }

        // ================= XỬ LÝ KÉO THẢ (DRAG & DROP) =================

        // Khi nhấn và di chuyển chuột trên danh sách file bên trái
        private void lstServerFiles_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is ListBox listBox && listBox.SelectedItem != null)
                {
                    // Lấy tên file và bắt đầu hiệu ứng kéo
                    string fileName = listBox.SelectedItem.ToString();
                    DragDrop.DoDragDrop(listBox, fileName, DragDropEffects.Copy);
                }
            }
        }

        // Khi thả file vào vùng bên phải (Download Queue)
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string fileName = (string)e.Data.GetData(DataFormats.StringFormat);

                // Kiểm tra nếu file này đang trong hàng đợi và chưa xong thì không thêm trùng
                foreach (var existing in Downloads)
                {
                    if (existing.FileName == fileName && !existing.IsCompleted)
                    {
                        MessageBox.Show("File này đã có trong danh sách tải!", "Thông báo");
                        return;
                    }
                }

                // Tạo item mới, thêm vào danh sách và bắt đầu tải
                var newItem = new DownloadItem(fileName);
                Downloads.Add(newItem);
                StartDownloadTask(newItem);
            }
        }

        // ================= LOGIC ĐIỀU KHIỂN TẢI FILE =================

        private async void StartDownloadTask(DownloadItem item)
        {
            var manager = new DownloadManager();

            // Vòng lặp chạy cho đến khi tải xong 100%
            while (item.Progress < 100 && !item.IsCompleted)
            {
                // Nếu người dùng nhấn Pause (IsPaused = true)
                if (item.IsPaused)
                {
                    item.Speed = 0; // Đưa tốc độ về 0 khi tạm dừng
                    await Task.Delay(500); // Đợi nửa giây rồi kiểm tra lại
                    continue;
                }

                // Mỗi lần Resume (hoặc bắt đầu mới), tạo một CancellationTokenSource mới
                item.Cts = new CancellationTokenSource();

                // Dùng Progress để cập nhật dữ liệu từ luồng phụ về UI một cách an toàn
                var progressReporter = new Progress<int>(p => item.Progress = p);
                var speedReporter = new Progress<double>(s => item.Speed = s);

                try
                {
                    // Gọi logic tải file từ DownloadManager
                    await manager.DownloadFile(
                        item.FileName,
                        progressReporter,
                        speedReporter,
                        item.Cts.Token,
                        item.Progress
                    );
                }
                catch (OperationCanceledException)
                {
                    // Xử lý khi nhấn nút Pause (Token bị hủy)
                    item.Speed = 0;
                }
                catch (Exception ex)
                {
                    item.Status = "Error!";
                    MessageBox.Show($"Lỗi tải file {item.FileName}: {ex.Message}");
                    break;
                }

                // Kiểm tra nếu đã hoàn thành
                if (item.Progress >= 100)
                {
                    item.IsCompleted = true;
                    item.Status = "Completed";
                    item.Speed = 0;
                }
            }
        }
    }
}