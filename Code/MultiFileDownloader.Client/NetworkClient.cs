using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Sockets;
using MultiFileDownloader.Shared;

namespace MultiFileDownloader.Client
{
    public class NetworkClient
    {
        TcpClient client;
        NetworkStream stream;

        public async Task Connect()
        {
            client = new TcpClient();

            await client.ConnectAsync("127.0.0.1", 8888);

            stream = client.GetStream();
        }

        public async Task RequestFileList()
        {
            byte[] packet = PacketHelper.CreatePacket(Command.RequestFileList, Array.Empty<byte>());

            await stream.WriteAsync(packet);
        }

        public async Task RequestDownload(string name)
        {
            byte[] payload = Encoding.UTF8.GetBytes(name);

            byte[] packet = PacketHelper.CreatePacket(Command.RequestDownload, payload);

            await stream.WriteAsync(packet);
        }

        public NetworkStream GetStream()
        {
            return stream;
        }

        public async Task<string[]> ReceiveFileList()
        {
            byte[] header = await NetworkUtils.ReadExactlyAsync(stream, 5);

            Command cmd = (Command)header[0];

            int length = PacketHelper.ParseLength(header);

            byte[] payload = await NetworkUtils.ReadExactlyAsync(stream, length);

            if (cmd != Command.SendFileList)
                return Array.Empty<string>();

            string data = Encoding.UTF8.GetString(payload);

            return data.Split('|');
        }
    }
}
