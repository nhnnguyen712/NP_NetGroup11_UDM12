using System.Windows;

namespace MultiFileDownloader.Client
{
    public partial class MainWindow : Window
    {
        NetworkClient client = new NetworkClient();

        public MainWindow()
        {
            InitializeComponent();

            _ = client.Connect();
        }

        private async void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            await client.RequestFileList();

            var files = await client.ReceiveFileList();

            fileList.Items.Clear();

            foreach (var f in files)
                fileList.Items.Add(f);
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (fileList.SelectedItem == null)
                return;

            string file = fileList.SelectedItem.ToString();

            await client.RequestDownload(file);

            DownloadManager manager = new DownloadManager();

            await manager.Download(client.GetStream(), file);
        }
    }
}
