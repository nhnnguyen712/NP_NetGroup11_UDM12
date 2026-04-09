using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiFileDownloader.Client
{
    public class DownloadItem : INotifyPropertyChanged
    {
        double progress = 0;
        string speed = "";

        public string FileName { get; set; }

        public long TotalSize { get; set; }

        public long ReceivedBytes { get; set; }

        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        public string Speed
        {
            get => speed;
            set
            {
                speed = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}