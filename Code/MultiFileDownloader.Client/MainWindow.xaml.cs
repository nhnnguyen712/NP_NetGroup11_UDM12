using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic;

namespace MultiFileDownloader.Client
{
    public partial class MainWindow : Window
    {
        NetworkClient client = new NetworkClient();

        ObservableCollection<DownloadItem> downloads =
            new ObservableCollection<DownloadItem>();

        public MainWindow()
        {
            InitializeComponent();

            lvDownloads.ItemsSource = downloads;

            _ = InitializeApp();
        }

        async Task InitializeApp()
        {
            try
            {
                await client.Connect();

                await LoadServerFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot connect server:\n" + ex.Message);
            }
        }

        async Task LoadServerFiles()
        {
            await client.RequestFileList();

            var files = await client.ReceiveFileList();

            lbServerFiles.Items.Clear();

            foreach (var f in files)
                lbServerFiles.Items.Add(f);
        }

        // DOWNLOAD BUTTON

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (lbServerFiles.SelectedItems.Count == 0)
                return;

            foreach (string file in lbServerFiles.SelectedItems)
            {
                _ = StartNewDownload(file);
            }
        }

        // DOWNLOAD LOGIC

        async Task StartNewDownload(string fileName)
        {
            string downloadFolder =
                Path.Combine(AppContext.BaseDirectory, "Downloads");

            Directory.CreateDirectory(downloadFolder);

            string extension = Path.GetExtension(fileName);

            string nameWithoutExt =
                Path.GetFileNameWithoutExtension(fileName);

            string saveName = fileName;

            string path = Path.Combine(downloadFolder, saveName);

            if (File.Exists(path))
            {
                var result = MessageBox.Show(
                    $"File '{fileName}' already exists.\n\n" +
                    "Yes = Rename and download\n" +
                    "No = Skip download",
                    "File Exists",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                    return;

                string newName = Interaction.InputBox(
                    "Enter new file name (without extension):",
                    "Rename File",
                    nameWithoutExt + "_1");

                if (string.IsNullOrWhiteSpace(newName))
                    return;

                saveName = newName + extension;
            }

            var item = new DownloadItem
            {
                FileName = saveName,
                Progress = 0,
                Speed = "Downloading..."
            };

            downloads.Add(item);

            await client.RequestDownload(fileName);

            DownloadManager manager = new DownloadManager();

            await manager.Download(client.GetStream(), saveName, item);
        }

        // OPEN DOWNLOAD FOLDER

        private void OpenDownloads_Click(object sender, RoutedEventArgs e)
        {
            string path =
                Path.Combine(AppContext.BaseDirectory, "Downloads");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Process.Start("explorer.exe", path);
        }

        // DRAG FILES

        private void lbServerFiles_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                lbServerFiles.SelectedItems.Count > 0)
            {
                var files = lbServerFiles.SelectedItems
                    .Cast<string>()
                    .ToArray();

                DragDrop.DoDragDrop(
                    lbServerFiles,
                    files,
                    DragDropEffects.Copy);
            }
        }

        // DROP FILES

        private void lvDownloads_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string[])))
            {
                string[] files =
                    (string[])e.Data.GetData(typeof(string[]));

                foreach (var file in files)
                {
                    _ = StartNewDownload(file);
                }
            }
        }
    }
}