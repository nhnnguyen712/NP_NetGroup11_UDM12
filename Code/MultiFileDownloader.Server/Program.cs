using System.Net;
using System.Net.Sockets;

namespace MultiFileDownloader.Server
{
    class Program
    {
        static async Task Main()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 8888);

            server.Start();

            Console.WriteLine("Server running at port 8888");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();

                Console.WriteLine("Client connected");

                _ = Task.Run(() => ClientHandler.Handle(client));
            }
        }
    }
}