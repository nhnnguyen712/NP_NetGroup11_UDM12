using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;

public class NetworkClient
{
    private TcpClient client;
    private NetworkStream stream;

    public void Connect(string host, int port)
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            Console.WriteLine("[Client] Đã kết nối");
        }
        catch (Exception e)
        {
            Console.WriteLine("[Client] Kết nối thất bại: " + e.Message);
        }
    }

    // gửi request lấy list file (command 1)
    public void RequestFileList()
    {
        SendPacket(1, new byte[0]);
    }

    // gửi request download (command 2)
    public void RequestDownload(string fileName)
    {
        byte[] data = Encoding.UTF8.GetBytes(fileName);
        SendPacket(2, data);
    }

    // nhận list file
    public List<string> ReceiveFileList()
    {
        List<string> result = new List<string>();

        try
        {
            byte cmd = ReadByte();
            int len = ReadInt();

            Console.WriteLine("[Debug] cmd=" + cmd + " len=" + len);

            if (cmd != 3)
            {
                Console.WriteLine("[Client] Sai command, expected 3");
                return result;
            }

            byte[] payload = ReadExact(len);
            string text = Encoding.UTF8.GetString(payload);

            Console.WriteLine("[Debug] raw=" + text);

            string[] parts = text.Split('|');

            foreach (var f in parts)
            {
                if (!string.IsNullOrWhiteSpace(f))
                    result.Add(f);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[Client] Lỗi khi nhận dữ liệu: " + e.Message);
        }

        return result;
    }

    // nhận file (ghi ra disk luôn)
    public void ReceiveFile(string savePath)
    {
        try
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                while (true)
                {
                    byte cmd = ReadByte();
                    int len = ReadInt();

                    if (cmd == 4) // chunk
                    {
                        byte[] data = ReadExact(len);
                        fs.Write(data, 0, data.Length);
                        Console.WriteLine("[Client] Đã nhận chunk: " + len);
                    }
                    else if (cmd == 5) // done
                    {
                        Console.WriteLine("[Client] Tải xong");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("[Client] Lệnh không xác định: " + cmd);
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[Client] Lỗi khi nhận file: " + e.Message);
        }
    }

    // ================== HELPER ==================

    private void SendPacket(byte command, byte[] payload)
    {
        try
        {
            if (stream == null) return;

            // command
            stream.WriteByte(command);

            // length (big endian)
            int len = payload.Length;
            int netLen = IPAddress.HostToNetworkOrder(len);
            byte[] lenBytes = BitConverter.GetBytes(netLen);
            stream.Write(lenBytes, 0, 4);

            // payload
            if (len > 0)
                stream.Write(payload, 0, len);

            Console.WriteLine("[Client] Sent cmd=" + command);
        }
        catch (Exception e)
        {
            Console.WriteLine("[Client] Send error: " + e.Message);
        }
    }

    private byte ReadByte()
    {
        int val = stream.ReadByte();
        if (val == -1) throw new Exception("Disconnected");
        return (byte)val;
    }

    private int ReadInt()
    {
        byte[] buf = ReadExact(4);
        int net = BitConverter.ToInt32(buf, 0);
        return IPAddress.NetworkToHostOrder(net);
    }

    // đọc đủ n bytes (quan trọng, không là lỗi ngay)
    private byte[] ReadExact(int size)
    {
        byte[] data = new byte[size];
        int total = 0;

        while (total < size)
        {
            int read = stream.Read(data, total, size - total);
            if (read <= 0) throw new Exception("Lost connection");
            total += read;
        }

        return data;
    }

    public void Close()
    {
        stream?.Close();
        client?.Close();
        Console.WriteLine("[Client] Closed");
    }
}
