using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace MultiFileDownloader.Client
{
    // Represents a file entry from the server list (with selection state for UI binding)
    public class ServerFileItem : INotifyPropertyChanged
    {
        private bool isSelected;

        public string FileName { get; set; } = "";

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
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