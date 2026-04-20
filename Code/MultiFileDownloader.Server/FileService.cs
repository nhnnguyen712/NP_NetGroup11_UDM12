using MultiFileDownloader.Shared;
using System.Net.Sockets;
using System.Text;

namespace MultiFileDownloader.Server
{
    // Handles file listing and file streaming to clients
    public static class FileService
    {
        static string root =
            Path.Combine(AppContext.BaseDirectory, "files");

        // Ensure the files directory exists on startup
        static FileService()
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
                Console.WriteLine($"Initialized folder: {root}");
            }
        }

        // Call this method on startup to trigger the static constructor immediately
        public static void Initialize() { }

        // Send pipe-delimited file list to client
        public static async Task SendFileList(NetworkStream stream)
        {
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
                Console.WriteLine($"Created folder: {root}");
            }

            var files = Directory.GetFiles(root)
                .Select(Path.GetFileName);

            string list = string.Join("|", files);

            byte[] payload = Encoding.UTF8.GetBytes(list);

            byte[] packet =
                PacketHelper.CreatePacket(Command.SendFileList, payload);

            await stream.WriteAsync(packet);
        }

        // Stream file to client: size -> chunks -> complete signal
        public static async Task SendFile(NetworkStream stream, string fileName)
        {
            string path = Path.Combine(root, fileName);

            if (!File.Exists(path))
                return;

            // Send file size
            long size = new FileInfo(path).Length;

            byte[] sizePacket =
                PacketHelper.CreatePacket(
                    Command.SendFileSize,
                    BitConverter.GetBytes(size));

            await stream.WriteAsync(sizePacket);

            // Send file data in 16KB chunks
            byte[] buffer = new byte[16384];

            // Open with Read access and shared Read to allow concurrent downloads
            using FileStream fs = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read);

            int read;

            while ((read = await fs.ReadAsync(buffer)) > 0)
            {
                byte[] chunk = new byte[read];
                Buffer.BlockCopy(buffer, 0, chunk, 0, read);

                byte[] packet =
                    PacketHelper.CreatePacket(Command.SendFileChunk, chunk);

                await stream.WriteAsync(packet);
            }

            // Send completion signal
            byte[] end =
                PacketHelper.CreatePacket(
                    Command.DownloadComplete,
                    Array.Empty<byte>());

            await stream.WriteAsync(end);
        }
    }
}