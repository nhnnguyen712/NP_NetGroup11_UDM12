using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ClientHandler
{
    public static async Task Handle(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        try
        {
            while (true)
            {
                // đọc 5 byte header
                byte[] header = await NetworkUtils.ReadExactlyAsync(stream, 5);

                // lấy command
                Command cmd = (Command)header[0];

                // lấy length
                int length = PacketHelper.ParseLength(header);

                // đọc payload
                byte[] payload = await NetworkUtils.ReadExactlyAsync(stream, length);

                Console.WriteLine($"Client: {client.Client.RemoteEndPoint} | CMD: {cmd} | LEN: {length}");

                // xử lý lệnh
                switch (cmd)
                {
                    case Command.RequestFileList:
                        Console.WriteLine("Client yêu cầu danh sách file");
                        break;

                    case Command.RequestDownload:
                        string fileName = Encoding.UTF8.GetString(payload);
                        Console.WriteLine("Client muốn tải: " + fileName);
                        break;

                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Client disconnected");
        }
        finally
        {
            client.Close();
        }
    }
}