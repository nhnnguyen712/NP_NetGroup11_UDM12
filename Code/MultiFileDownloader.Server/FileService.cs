using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiFileDownloader.Server
{
    public class FileService
    {
        private readonly string _serverFolder;

        public FileService(string serverFolder)
        {
            _serverFolder = serverFolder;
            if (!Directory.Exists(_serverFolder))
            {
                Directory.CreateDirectory(_serverFolder);
            }
        }

        // SendFileList()
        // Nhiệm vụ: Directory.GetFiles, string.Join("|", ...)
        // Phụ: bảo mật: Path.GetFileName()
        public void SendFileList(NetworkStream stream)
        {
            // Lấy danh sách các file trong thư mục
            string[] files = Directory.GetFiles(_serverFolder);

            // Bảo mật: Lấy tên file thay vì đường dẫn tuyệt đối
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

            // string.Join("|", ...) để nối danh sách file
            string fileListString = string.Join("|", files);

            // Chuyển string sang mảng byte UTF-8
            byte[] payload = Encoding.UTF8.GetBytes(fileListString);

            // Gửi packet SendFileList này sang NetworkStream
            // Dùng hàm SendPacket nội bộ hoặc dùng PacketHelper từ Shared project
            SendPacket(stream, 3, payload);
        }

        // SendFile()
        // Nhiệm vụ: chunk 4096, gửi DownloadComplete, bảo mật: Path.GetFileName()
        public void SendFile(NetworkStream stream, string requestedFileName)
        {
            try
            {
                // Bảo mật: Sử dụng Path.GetFileName để tránh lỗi bảo mật Path Traversal
                string safeFileName = Path.GetFileName(requestedFileName);
                string filePath = Path.Combine(_serverFolder, safeFileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[Error] File không tồn tại: {safeFileName}");
                    return;
                }

                // Nhiệm vụ: chunk 4096
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        byte[] payload = new byte[bytesRead];
                        Array.Copy(buffer, payload, bytesRead);

                        // Gửi từng chunk tới client (Command: SendFile, ví dụ byte 4)
                        SendPacket(stream, 4, payload);
                    }
                }

                // Gửi DownloadComplete (Command: DownloadComplete, ví dụ byte 5)
                // Payload rỗng vì file đã truyền xong
                SendPacket(stream, 5, Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Lỗi khi xử lý SendFile: {ex.Message}");
            }
        }

        // Hàm phụ trợ đóng gói packet theo quy ước từ Rules Protocol 
        // [Command (1 byte)] [Length (4 bytes)] [Payload (n bytes)]
        private void SendPacket(NetworkStream stream, byte command, byte[] payload)
        {
            // Command (1 byte)
            stream.WriteByte(command);

            // Length (4 bytes) - IPAddress.HostToNetworkOrder (Network Byte Order = Big Endian)
            int length = payload.Length;
            int networkOrderLength = IPAddress.HostToNetworkOrder(length);
            byte[] lengthBytes = BitConverter.GetBytes(networkOrderLength);
            stream.Write(lengthBytes, 0, 4);

            // Payload (n bytes)
            if (payload.Length > 0)
            {
                stream.Write(payload, 0, payload.Length);
            }
        }
    }
}
