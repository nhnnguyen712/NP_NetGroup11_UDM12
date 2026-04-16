using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiFileDownloader.Client
{
    public partial class MainWindow : Window
    {
        NetworkClient client;

        ObservableCollection<DownloadItem> downloads =
            new ObservableCollection<DownloadItem>();

        SemaphoreSlim semaphore = new SemaphoreSlim(3);

        int activeDownloads = 0;

        // Cho Ctrl/Shift Click functionality
        private ServerFileItem lastSelectedItem = null;
        private ServerFileItem itemBeingClicked = null;

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
                // Hiển thị dialog để nhập Server Address
                string serverAddress = await ShowServerConnectionDialog();

                if (string.IsNullOrEmpty(serverAddress))
                {
                    MessageBox.Show("Connection cancelled");
                    this.Close();
                    return;
                }

                // Parse host:port
                var parts = serverAddress.Split(':');
                string host = parts[0];
                int port = parts.Length > 1 && int.TryParse(parts[1], out int p) ? p : 8888;

                // Tạo client với server address
                client = new NetworkClient(host, port);

                await client.Connect();
                await LoadServerFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message);
                this.Close();
            }
        }

        async Task<string> ShowServerConnectionDialog()
        {
            var dialog = new Window
            {
                Title = "Connect to Server",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 14
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20), VerticalAlignment = VerticalAlignment.Center };

            var labelHost = new TextBlock 
            { 
                Text = "Server Address (host:port):", 
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8),
                Foreground = new SolidColorBrush(Color.FromRgb(30, 136, 229))
            };

            var textboxHost = new TextBox 
            { 
                Text = "127.0.0.1:8888",
                Padding = new Thickness(10),
                Height = 40,
                Margin = new Thickness(0, 0, 0, 15),
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(1)
            };

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };

            var buttonConnect = new Button 
            { 
                Content = "Connect",
                Width = 100,
                Padding = new Thickness(10, 8, 10, 8),
                Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(5)
            };

            var buttonCancel = new Button 
            { 
                Content = "Cancel",
                Width = 100,
                Padding = new Thickness(10, 8, 10, 8),
                Background = new SolidColorBrush(Color.FromRgb(189, 189, 189)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(5)
            };

            string result = null;
            buttonConnect.Click += (s, e) => 
            { 
                result = textboxHost.Text;
                dialog.Close();
            };
            buttonCancel.Click += (s, e) => dialog.Close();

            buttonPanel.Children.Add(buttonConnect);
            buttonPanel.Children.Add(buttonCancel);

            mainPanel.Children.Add(labelHost);
            mainPanel.Children.Add(textboxHost);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();

            return result;
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

        // ================= MULTIPLE SELECTION =================

        private void lbServerFiles_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = GetItemAtPoint(lbServerFiles, e.GetPosition(lbServerFiles));

            if (item != null)
            {
                itemBeingClicked = item; // Track item được click

                // Kiểm tra nếu click vào checkbox, để nó handle bình thường
                if (IsClickOnCheckBox(e, item))
                {
                    lastSelectedItem = item;
                    return; // Không handle, để checkbox tự xử lý
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    // Ctrl+Click: Toggle selection
                    item.IsSelected = !item.IsSelected;
                    lastSelectedItem = item;
                    e.Handled = true;
                }
                else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    // Shift+Click: Range selection
                    if (lastSelectedItem != null)
                    {
                        SelectRange(lastSelectedItem, item);
                    }
                    else
                    {
                        item.IsSelected = true;
                        lastSelectedItem = item;
                    }
                    e.Handled = true;
                }
                else
                {
                    // Normal click vào text (không phải checkbox): Toggle selection
                    item.IsSelected = !item.IsSelected;
                    lastSelectedItem = item;
                    e.Handled = true;
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is ServerFileItem item)
            {
                lastSelectedItem = item;
            }
        }

        private void lbServerFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handled by PreviewMouseLeftButtonDown
        }

        private void SelectRange(ServerFileItem from, ServerFileItem to)
        {
            int indexFrom = lbServerFiles.Items.IndexOf(from);
            int indexTo = lbServerFiles.Items.IndexOf(to);

            if (indexFrom > indexTo)
            {
                (indexFrom, indexTo) = (indexTo, indexFrom);
            }

            for (int i = 0; i < lbServerFiles.Items.Count; i++)
            {
                if (lbServerFiles.Items[i] is ServerFileItem item)
                {
                    item.IsSelected = (i >= indexFrom && i <= indexTo);
                }
            }
        }

        private ServerFileItem GetItemAtPoint(ListBox listBox, Point point)
        {
            var result = VisualTreeHelper.HitTest(listBox, point);
            var visual = result?.VisualHit;

            while (visual != null)
            {
                if (VisualTreeHelper.GetParent(visual) is ListBoxItem itemContainer)
                {
                    return listBox.ItemContainerGenerator.ItemFromContainer(itemContainer) as ServerFileItem;
                }
                visual = VisualTreeHelper.GetParent(visual) as Visual;
            }

            return null;
        }

        private bool IsClickOnCheckBox(MouseButtonEventArgs e, ServerFileItem item)
        {
            var hitTest = VisualTreeHelper.HitTest(lbServerFiles, e.GetPosition(lbServerFiles));
            var visual = hitTest?.VisualHit;

            while (visual != null)
            {
                if (visual is CheckBox)
                    return true;

                visual = VisualTreeHelper.GetParent(visual) as Visual;
            }

            return false;
        }

        // ================= DRAG & DROP =================

        private void lbServerFiles_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var item = GetItemAtPoint(lbServerFiles, e.GetPosition(lbServerFiles));

                if (item != null && itemBeingClicked == item)
                {
                    // Khi detect drag bắt đầu, nếu item không được select, tự động select nó
                    // (Để drag & drop hoạt động, ngay cả khi user vừa toggle untick)
                    if (!item.IsSelected)
                    {
                        item.IsSelected = true;
                    }

                    // Lấy tất cả selected items
                    var selectedFiles = lbServerFiles.Items
                        .OfType<ServerFileItem>()
                        .Where(x => x.IsSelected)
                        .Select(x => x.FileName)
                        .ToList();

                    if (selectedFiles.Count > 0)
                    {
                        var dataObject = new DataObject();
                        dataObject.SetData("ServerFiles", selectedFiles);

                        DragDrop.DoDragDrop(lbServerFiles, dataObject, DragDropEffects.Copy);

                        itemBeingClicked = null; // Reset
                    }
                }
            }
        }

        private void lvDownloads_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ServerFiles"))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void lvDownloads_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData("ServerFiles") is List<string> files)
            {
                foreach (var fileName in files)
                {
                    _ = StartNewDownload(fileName);
                }
                e.Handled = true;
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

        private void ClearAll()
        {
            foreach (ServerFileItem item in lbServerFiles.Items)
                item.IsSelected = false;
        }
    }
}