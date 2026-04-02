using MultiFileDownloader.Shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MultiFileDownloader.Server
{

    public static class FileService
    {
        static string root = Path.Combine(AppContext.BaseDirectory, "files");

        static FileService()
        {
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
        }

        static string Sanitize(string name)
        {
            return Path.GetFileName(name);
        }

        public static async Task SendFileList(NetworkStream stream)
        {
            string[] files = Directory.GetFiles(root);

            string list = string.Join("|", files.Select(Path.GetFileName));

            byte[] payload = Encoding.UTF8.GetBytes(list);

            byte[] packet = PacketHelper.CreatePacket(Command.SendFileList, payload);

            await stream.WriteAsync(packet);
        }

        public static async Task SendFile(NetworkStream stream, string name)
        {
            name = Sanitize(name);

            string path = Path.Combine(root, name);

            if (!File.Exists(path))
                return;

            byte[] buffer = new byte[4096];

            using FileStream fs = new FileStream(path, FileMode.Open);

            int read;

            while ((read = await fs.ReadAsync(buffer)) > 0)
            {
                byte[] chunk = buffer.Take(read).ToArray();

                byte[] packet = PacketHelper.CreatePacket(Command.SendFileChunk, chunk);

                await stream.WriteAsync(packet);
            }

            byte[] end = PacketHelper.CreatePacket(Command.DownloadComplete, Array.Empty<byte>());

            await stream.WriteAsync(end);
        }
    }
}

