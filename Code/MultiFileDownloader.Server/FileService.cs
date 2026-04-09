using MultiFileDownloader.Shared;
using System.Net.Sockets;
using System.Text;

namespace MultiFileDownloader.Server
{
    public static class FileService
    {
        static string root =
            Path.Combine(AppContext.BaseDirectory, "files");

        public static async Task SendFileList(NetworkStream stream)
        {
            var files = Directory.GetFiles(root)
                .Select(Path.GetFileName);

            string list = string.Join("|", files);

            byte[] payload = Encoding.UTF8.GetBytes(list);

            byte[] packet =
                PacketHelper.CreatePacket(Command.SendFileList, payload);

            await stream.WriteAsync(packet);
        }

        public static async Task SendFile(NetworkStream stream, string fileName)
        {
            string path = Path.Combine(root, fileName);

            if (!File.Exists(path))
                return;

            long size = new FileInfo(path).Length;

            byte[] sizePacket =
                PacketHelper.CreatePacket(
                    Command.SendFileSize,
                    BitConverter.GetBytes(size));

            await stream.WriteAsync(sizePacket);

            byte[] buffer = new byte[8192];

            using FileStream fs = new FileStream(path, FileMode.Open);

            int read;

            while ((read = await fs.ReadAsync(buffer)) > 0)
            {
                byte[] chunk = buffer.Take(read).ToArray();

                byte[] packet =
                    PacketHelper.CreatePacket(Command.SendFileChunk, chunk);

                await stream.WriteAsync(packet);
            }

            byte[] end =
                PacketHelper.CreatePacket(
                    Command.DownloadComplete,
                    Array.Empty<byte>());

            await stream.WriteAsync(end);
        }
    }
}