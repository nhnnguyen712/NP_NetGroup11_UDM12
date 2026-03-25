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

            // fake dữ liệu server
            lstServerFiles.Items.Add("file1.zip");
            lstServerFiles.Items.Add("video.mp4");
            lstServerFiles.Items.Add("music.mp3");

            Downloads = new ObservableCollection<DownloadItem>();
            lstDownloads.ItemsSource = Downloads;
        }

        // kéo file
        private void lstServerFiles_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var listBox = sender as ListBox;
                var item = listBox.SelectedItem;

                if (item != null)
                {
                    DragDrop.DoDragDrop(listBox, item.ToString(), DragDropEffects.Copy);
                }
            }
        }

        // thả file
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string file = e.Data.GetData(typeof(string)) as string;

            if (file != null)
            {
                Downloads.Add(new DownloadItem
                {
                    FileName = file,
                    Progress = 0,
                    Speed = 0,
                    Status = "Waiting"
                });
            }
        }
    }
}