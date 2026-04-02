using MultiFileDownloader.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace MultiFileDownloader.Client
{
    public class DownloadManager
    {
        public async Task Download(NetworkStream stream, string file)
        {
            string downloadFolder = Path.Combine(AppContext.BaseDirectory, "Downloads");

            if (!Directory.Exists(downloadFolder))
                Directory.CreateDirectory(downloadFolder);

            string fullPath = Path.Combine(downloadFolder, file);

            using FileStream fs = new FileStream(fullPath, FileMode.Create);
            

            while (true)
            {
                byte[] header = await NetworkUtils.ReadExactlyAsync(stream, 5);

                Command cmd = (Command)header[0];

                int length = PacketHelper.ParseLength(header);

                byte[] payload = await NetworkUtils.ReadExactlyAsync(stream, length);

                if (cmd == Command.SendFileChunk)
                {
                    await fs.WriteAsync(payload);
                }

                if (cmd == Command.DownloadComplete)
                {
                    break;
                }
            }
        }
    }
}
