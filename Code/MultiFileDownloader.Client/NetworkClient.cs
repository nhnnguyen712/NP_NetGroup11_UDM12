using System.Net.Sockets;
using System.Text;
using MultiFileDownloader.Shared;

namespace MultiFileDownloader.Client
{
    public class NetworkClient
    {
        TcpClient client;
        NetworkStream stream;
        private string serverHost = "127.0.0.1";
        private int serverPort = 8888;

        // Constructor để set server address
        public NetworkClient(string host = "127.0.0.1", int port = 8888)
        {
            serverHost = host;
            serverPort = port;
        }

        public async Task<TcpClient> CreateNewConnection()
        {
            TcpClient client = new TcpClient();

            await client.ConnectAsync(serverHost, serverPort);

            return client;
        }

        public async Task SendDownloadRequest(NetworkStream stream, string file)
        {
            byte[] payload = Encoding.UTF8.GetBytes(file);

            byte[] packet =
                PacketHelper.CreatePacket(
                    Command.RequestDownload,
                    payload);

            await stream.WriteAsync(packet);
        }

        public async Task Connect()
        {
            client = new TcpClient();

            await client.ConnectAsync(serverHost, serverPort);

            stream = client.GetStream();
        }

        public async Task RequestFileList()
        {
            byte[] packet =
                PacketHelper.CreatePacket(
                    Command.RequestFileList,
                    Array.Empty<byte>());

            await stream.WriteAsync(packet);
        }

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