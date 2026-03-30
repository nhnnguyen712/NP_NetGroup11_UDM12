using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading;

namespace MultiFileDownloader.Client
{
    public class DownloadItem : INotifyPropertyChanged
    {
        // Tên file (ReadOnly)
        public string FileName { get; }

        // ===== PROGRESS (0-100) =====
        private int _progress;
        public int Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        // ===== SPEED (MB/s hoặc KB/s) =====
        private double _speed;
        public double Speed
        {
            get => _speed;
            set { _speed = value; OnPropertyChanged(); }
        }

        // ===== STATUS (Downloading, Paused, Completed, Error) =====
        private string _status = "Downloading...";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        // ===== COMPLETED =====
        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set 
            { 
                _isCompleted = value; 
                if (value) Status = "Completed";
                OnPropertyChanged(); 
            }
        }

        // ===== PAUSE/RESUME LOGIC =====
        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PauseText));
                    
                    if (!IsCompleted) 
                        Status = _isPaused ? "Paused" : "Downloading...";
                }
            }
        }

        // Text hiển thị trên nút bấm
        public string PauseText => IsPaused ? "Resume" : "Pause";

        // Token để quản lý việc hủy/tạm dừng Task
        // Sử dụng dấu ? để tránh cảnh báo CS8618 (Nullability)
        public CancellationTokenSource? Cts { get; set; }

        // Command cho nút bấm
        public ICommand TogglePauseCommand { get; }

        public DownloadItem(string name)
        {
            FileName = name;
            Cts = new CancellationTokenSource();
            
            // Khởi tạo command thông qua RelayCommand
            TogglePauseCommand = new RelayCommand(TogglePause);
        }

        private void TogglePause()
        {
            if (IsCompleted) return;
            IsPaused = !IsPaused;

            // Nếu người dùng nhấn Pause, chúng ta hủy Token hiện tại
            if (IsPaused)
            {
                Cts?.Cancel();
            }
        }

        // ===== NOTIFY UI (Sửa lỗi Warning CS8612) =====
        // Thêm dấu ? vào PropertyChangedEventHandler để khớp với định nghĩa của .NET mới
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}