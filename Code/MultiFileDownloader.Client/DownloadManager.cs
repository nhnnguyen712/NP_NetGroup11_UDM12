using MultiFileDownloader.Shared;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;

namespace MultiFileDownloader.Client
{
    public class DownloadManager
    {
        string downloadFolder =
            Path.Combine(AppContext.BaseDirectory, "Downloads");

        public DownloadManager()
        {
            Directory.CreateDirectory(downloadFolder);
        }

        public async Task Download(
    NetworkStream stream,
    string fileName,
    DownloadItem item)
        {
            string path = Path.Combine(downloadFolder, fileName);

            using FileStream fs = new FileStream(path, FileMode.Create);

            long received = 0;

            long lastBytes = 0;
            long lastTime = Environment.TickCount;

            while (true)
            {
                byte[] header =
                    await NetworkUtils.ReadExactlyAsync(stream, 5);

                Command cmd = (Command)header[0];

                int length =
                    PacketHelper.ParseLength(header);

                byte[] payload =
                    await NetworkUtils.ReadExactlyAsync(stream, length);

                if (cmd == Command.SendFileSize)
                {
                    item.TotalSize = BitConverter.ToInt64(payload);
                }

                if (cmd == Command.SendFileChunk)
                {
                    await fs.WriteAsync(payload);

                    received += payload.Length;

                    item.ReceivedBytes = received;

                    if (item.TotalSize > 0)
                    {
                        double progress =
                            (double)received /
                            item.TotalSize * 100;

                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            item.Progress = progress;
                        });
                    }

                    int now = Environment.TickCount;

                    if (now - lastTime >= 1000)
                    {
                        long bytesPerSec =
                            received - lastBytes;

                        double kb =
                            bytesPerSec / 1024.0;

                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            item.Speed = $"{kb:F1} KB/s";
                        });

                        lastBytes = received;
                        lastTime = now;
                    }
                }

                if (cmd == Command.DownloadComplete)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        item.Progress = 100;
                        item.Speed = "Completed";
                    });

                    break;
                }
            }
        }
    }
}