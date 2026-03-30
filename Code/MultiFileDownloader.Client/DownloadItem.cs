using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading;

namespace MultiFileDownloader.Client
{
    public class DownloadItem : INotifyPropertyChanged
    {
        public string FileName { get; set; }

        // ===== PROGRESS =====
        private int _progress;
        public int Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }

        // ===== SPEED =====
        private double _speed;
        public double Speed
        {
            get => _speed;
            set { _speed = value; OnPropertyChanged(); }
        }

        // ===== STATUS =====
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
            set { _isCompleted = value; OnPropertyChanged(); }
        }

        // ===== PAUSE =====
        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PauseText));
            }
        }

        // Text hiển thị nút
        public string PauseText => IsPaused ? "Resume" : "Pause";

        // Token
        public CancellationTokenSource Cts { get; set; }

        // Command
        public ICommand TogglePauseCommand { get; }

        public DownloadItem(string name)
        {
            FileName = name;
            Cts = new CancellationTokenSource();

            TogglePauseCommand = new RelayCommand(TogglePause);
        }

        private void TogglePause()
        {
            IsPaused = !IsPaused;
            Status = IsPaused ? "Paused" : "Downloading...";
        }

        // ===== NOTIFY UI =====
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}