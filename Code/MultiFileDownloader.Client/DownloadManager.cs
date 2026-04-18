using MultiFileDownloader.Shared;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;

namespace MultiFileDownloader.Client
{
    // Receives file data from server and writes to disk with progress tracking
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

            long lastUIUpdate = Environment.TickCount;
            long lastSpeedTime = Environment.TickCount;
            long lastBytes = 0;

            while (true)
            {
                byte[] header =
                    await NetworkUtils.ReadExactlyAsync(stream, 5);

                Command cmd = (Command)header[0];

                int length =
                    PacketHelper.ParseLength(header);

                byte[] payload =
                    await NetworkUtils.ReadExactlyAsync(stream, length);

                // Receive total file size
                if (cmd == Command.SendFileSize)
                {
                    item.TotalSize = BitConverter.ToInt64(payload);
                }

                // Receive file data chunk
                if (cmd == Command.SendFileChunk)
                {
                    await fs.WriteAsync(payload);

                    received += payload.Length;
                    item.ReceivedBytes = received;

                    long now = Environment.TickCount;

                    // Update progress bar (throttle: every 100ms)
                    if (now - lastUIUpdate > 100)
                    {
                        double progress =
                            item.TotalSize > 0
                            ? (double)received / item.TotalSize * 100
                            : 0;

                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            item.Progress = progress;
                        });

                        lastUIUpdate = now;
                    }

                    // Update speed display (throttle: every 1s)
                    if (now - lastSpeedTime > 1000)
                    {
                        long bytesPerSec = received - lastBytes;

                        double kb = bytesPerSec / 1024.0;

                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            item.Speed = $"{kb:F1} KB/s";
                        });

                        lastBytes = received;
                        lastSpeedTime = now;
                    }
                }

                // Download finished
                if (cmd == Command.DownloadComplete)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (item.Progress < 100)
                        {
                            item.Progress = 100;
                        }

                        item.Speed = "Completed";
                    });

                    break;
                }
            }
        }
    }
}