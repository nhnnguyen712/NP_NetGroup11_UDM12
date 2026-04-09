using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;

namespace MultiFileDownloader.Client
{
    public partial class MainWindow : Window
    {

        NetworkClient client = new NetworkClient();

        ObservableCollection<DownloadItem> downloads =
            new ObservableCollection<DownloadItem>();

        SemaphoreSlim semaphore = new SemaphoreSlim(Environment.ProcessorCount); // 👈 THÊM Ở ĐÂY


        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServerFileItem item in lbServerFiles.Items)
            {
                item.IsSelected = true;
            }

            lbServerFiles.Items.Refresh(); // cập nhật UI
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServerFileItem item in lbServerFiles.Items)
            {
                item.IsSelected = false;
            }

            lbServerFiles.Items.Refresh(); // cập nhật UI
        }


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
            {
                lbServerFiles.Items.Add(new ServerFileItem
                {
                    FileName = f,
                    IsSelected = false
                });
            }
        }

        // DOWNLOAD BUTTON

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (lbServerFiles.SelectedItems.Count == 0)
                return;

            foreach (ServerFileItem item in lbServerFiles.Items)
            {
                if (item.IsSelected)
                {
                    _ = StartNewDownload(item.FileName);
                }
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

            TcpClient newClient = await client.CreateNewConnection();

            NetworkStream stream = newClient.GetStream();

            await client.SendDownloadRequest(stream, fileName);

            DownloadManager manager = new DownloadManager();

            _ = Task.Run(async () =>
            {
                await semaphore.WaitAsync();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        TcpClient newClient = await client.CreateNewConnection();

                        NetworkStream stream = newClient.GetStream();

                        await client.SendDownloadRequest(stream, fileName);

                        DownloadManager manager = new DownloadManager();

                        await manager.Download(stream, saveName, item);

                        newClient.Close();
                    }
                    finally
                    {
                        semaphore.Release(); // 👈 cực kỳ quan trọng
                    }
                });
                newClient.Close();
            });
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