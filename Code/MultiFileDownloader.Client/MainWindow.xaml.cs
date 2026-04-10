using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace MultiFileDownloader.Client
{
    public partial class MainWindow : Window
    {
        NetworkClient client = new NetworkClient();

        ObservableCollection<DownloadItem> downloads =
            new ObservableCollection<DownloadItem>();

        SemaphoreSlim semaphore = new SemaphoreSlim(3);

        int activeDownloads = 0;

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
                MessageBox.Show(ex.Message);
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
                    FileName = f
                });
            }
        }

        // ================= DOWNLOAD =================

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServerFileItem item in lbServerFiles.Items)
            {
                if (item.IsSelected)
                {
                    _ = StartNewDownload(item.FileName);
                }
            }
        }

        async Task StartNewDownload(string fileName)
        {
            Interlocked.Increment(ref activeDownloads);

            string folder = Path.Combine(AppContext.BaseDirectory, "Downloads");
            Directory.CreateDirectory(folder);

            string saveName = fileName;

            string path = Path.Combine(folder, saveName);

            if (File.Exists(path))
            {
                var result = MessageBox.Show(
                    $"File '{fileName}' already exists\nRename?",
                    "File exists",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                    return;

                

                // 👉 CHỈ tăng khi CHẮC CHẮN download
                Interlocked.Increment(ref activeDownloads);

                string name = Path.GetFileNameWithoutExtension(fileName);
                string ext = Path.GetExtension(fileName);

                string newName = Interaction.InputBox(
                    "New name:",
                    "Rename",
                    name + "_1");

                if (string.IsNullOrWhiteSpace(newName))
                    return;

                saveName = newName + ext;
            }

            var item = new DownloadItem
            {
                FileName = saveName,
                Speed = "Waiting..."
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                downloads.Add(item);
            });

            await semaphore.WaitAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    TcpClient c = await client.CreateNewConnection();

                    NetworkStream stream = c.GetStream();

                    await client.SendDownloadRequest(stream, fileName);

                    DownloadManager manager = new DownloadManager();

                    await manager.Download(stream, saveName, item);

                    c.Close();
                }
                finally
                {
                    semaphore.Release();

                    if (Interlocked.Decrement(ref activeDownloads) == 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (ServerFileItem item in lbServerFiles.Items)
                            {
                                item.IsSelected = false;
                            }
                        });
                    }
                }
            });
        }

        void UntickAll()
        {
            foreach (ServerFileItem item in lbServerFiles.Items)
            {
                item.IsSelected = false;
            }
        }

        // ================= BUTTON =================

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServerFileItem item in lbServerFiles.Items)
                item.IsSelected = true;
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ServerFileItem item in lbServerFiles.Items)
                item.IsSelected = false;
        }

        private void OpenDownloads_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Downloads");

            Directory.CreateDirectory(path);

            Process.Start("explorer.exe", path);
        }
        private void lbServerFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ServerFileItem item in e.AddedItems)
            {
                item.IsSelected = true;
            }
        }
    }
}