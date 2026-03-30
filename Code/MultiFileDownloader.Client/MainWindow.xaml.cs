using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiFileDownloader.Client
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DownloadItem> Downloads { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Downloads = new ObservableCollection<DownloadItem>();
            DataContext = this;

            // 🔥 THÊM DÒNG NÀY (QUAN TRỌNG NHẤT)
            var testItem = new DownloadItem("test.pdf");
            Downloads.Add(testItem);
            testItem.StartDownload();

            // Fake server files
            lstServerFiles.Items.Add("📄 Document.pdf");
            lstServerFiles.Items.Add("🎬 Video.mp4");
            lstServerFiles.Items.Add("🎵 Music.mp3");
        }

        private void lstServerFiles_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is ListBox listBox && listBox.SelectedItem != null)
                {
                    DragDrop.DoDragDrop(
                        listBox,
                        listBox.SelectedItem.ToString(),
                        DragDropEffects.Copy
                    );
                }
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string fileName = e.Data.GetData(typeof(string)) as string;

            if (!string.IsNullOrEmpty(fileName))
            {
                var item = new DownloadItem(fileName);

                Downloads.Add(item);

                item.StartDownload();
            }
        }
    }
}