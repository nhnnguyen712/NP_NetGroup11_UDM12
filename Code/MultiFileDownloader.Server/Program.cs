using System.Net;
using System.Net.Sockets;

class Program
{
    static async Task Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 6000);
        listener.Start();

        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected");

            _ = Task.Run(() => ClientHandler.Handle(client));
        }
    }
}