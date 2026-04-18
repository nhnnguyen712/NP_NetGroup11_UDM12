using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using MultiFileDownloader.Shared;

namespace MultiFileDownloader.StressTest
{
    // Lightweight TCP client that mirrors the app protocol for automated testing
    public class TestClient : IDisposable
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private readonly string host;
        private readonly int port;

        public TestClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        // Establish TCP connection and return elapsed time in milliseconds
        public async Task<double> ConnectAsync()
        {
            client = new TcpClient();

            var sw = Stopwatch.StartNew();
            await client.ConnectAsync(host, port);
            sw.Stop();

            stream = client.GetStream();

            return sw.Elapsed.TotalMilliseconds;
        }

        // Request file list from server and return timing + file names
        public async Task<(double ElapsedMs, string[] Files)> GetFileListAsync()
        {
            if (stream == null)
                throw new InvalidOperationException("Not connected");

            var sw = Stopwatch.StartNew();

            // Send RequestFileList packet
            byte[] packet = PacketHelper.CreatePacket(
                Command.RequestFileList, Array.Empty<byte>());

            await stream.WriteAsync(packet);

            // Receive SendFileList response
            byte[] header = await NetworkUtils.ReadExactlyAsync(stream, 5);
            int len = PacketHelper.ParseLength(header);
            byte[] payload = await NetworkUtils.ReadExactlyAsync(stream, len);

            sw.Stop();

            string list = Encoding.UTF8.GetString(payload);

            string[] files = list.Split('|',
                StringSplitOptions.RemoveEmptyEntries);

            return (sw.Elapsed.TotalMilliseconds, files);
        }

        // Download a file and return timing + total bytes received (data is discarded)
        public async Task<(double ElapsedMs, long BytesReceived)> DownloadFileAsync(
            string fileName)
        {
            if (stream == null)
                throw new InvalidOperationException("Not connected");

            var sw = Stopwatch.StartNew();
            long totalBytes = 0;

            // Send RequestDownload packet
            byte[] namePayload = Encoding.UTF8.GetBytes(fileName);

            byte[] packet = PacketHelper.CreatePacket(
                Command.RequestDownload, namePayload);

            await stream.WriteAsync(packet);

            // Receive file chunks until DownloadComplete
            while (true)
            {
                byte[] header =
                    await NetworkUtils.ReadExactlyAsync(stream, 5);

                Command cmd = (Command)header[0];
                int length = PacketHelper.ParseLength(header);

                byte[] data =
                    await NetworkUtils.ReadExactlyAsync(stream, length);

                if (cmd == Command.SendFileChunk)
                {
                    totalBytes += data.Length;
                }
                else if (cmd == Command.DownloadComplete)
                {
                    break;
                }
            }

            sw.Stop();

            return (sw.Elapsed.TotalMilliseconds, totalBytes);
        }

        public void Dispose()
        {
            stream?.Dispose();
            client?.Close();
            client?.Dispose();
        }
    }
}
