using System.Net.Sockets;
using System.Text;
using MultiFileDownloader.Shared;

namespace MultiFileDownloader.Client
{
    // Manages TCP connection to the server for file listing and downloading
    public class NetworkClient
    {
        TcpClient client;
        NetworkStream stream;
        private string serverHost = "127.0.0.1";
        private int serverPort = 8888;

        public NetworkClient(string host = "127.0.0.1", int port = 8888)
        {
            serverHost = host;
            serverPort = port;
        }

        // Create a dedicated TCP connection for a single download
        public async Task<TcpClient> CreateNewConnection()
        {
            TcpClient client = new TcpClient();

            await client.ConnectAsync(serverHost, serverPort);

            return client;
        }

        // Send download request on a specific stream
        public async Task SendDownloadRequest(NetworkStream stream, string file)
        {
            byte[] payload = Encoding.UTF8.GetBytes(file);

            byte[] packet =
                PacketHelper.CreatePacket(
                    Command.RequestDownload,
                    payload);

            await stream.WriteAsync(packet);
        }

        // Establish the primary control connection
        public async Task Connect()
        {
            client = new TcpClient();

            await client.ConnectAsync(serverHost, serverPort);

            stream = client.GetStream();
        }

        // Request file list from server
        public async Task RequestFileList()
        {
            byte[] packet =
                PacketHelper.CreatePacket(
                    Command.RequestFileList,
                    Array.Empty<byte>());

            await stream.WriteAsync(packet);
        }

        // Receive and parse pipe-delimited file list
        public async Task<string[]> ReceiveFileList()
        {
            byte[] header =
                await NetworkUtils.ReadExactlyAsync(stream, 5);

            int len = PacketHelper.ParseLength(header);

            byte[] payload =
                await NetworkUtils.ReadExactlyAsync(stream, len);

            string list = Encoding.UTF8.GetString(payload);

            return list.Split('|');
        }

        // Send download request on the control stream
        public async Task RequestDownload(string file)
        {
            byte[] payload = Encoding.UTF8.GetBytes(file);

            byte[] packet =
                PacketHelper.CreatePacket(
                    Command.RequestDownload,
                    payload);

            await stream.WriteAsync(packet);
        }

        public NetworkStream GetStream()
        {
            return stream;
        }
    }
}