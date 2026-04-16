using System.Net;
using System.Net.Sockets;

namespace MultiFileDownloader.Server
{
    class Program
    {
        static async Task Main()
        {
            // Lấy IP address của server
            string serverIP = GetServerIP();

            // Bind tất cả IP addresses (0.0.0.0), không chỉ localhost
            TcpListener server = new TcpListener(IPAddress.Any, 8888);

            server.Start();

            // In thông tin server đẹp
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║        🔗 Multi File Downloader Server                    ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║  📍 Server IP (Localhost): 127.0.0.1:8888               ║");
            Console.WriteLine($"║  📍 Server IP (Network):   {serverIP}:8888".PadRight(60) + "║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  📋 Files Location: ./files                               ║");
            Console.WriteLine("║  ⏱️  Started at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(41) + "║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  ✅ Waiting for client connections...                     ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            int clientCount = 0;

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clientCount++;

                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ✓ Client #{clientCount} connected from {clientIP}");

                _ = Task.Run(() => ClientHandler.Handle(client));
            }
        }

        // Hàm lấy IP address của server
        static string GetServerIP()
        {
            try
            {
                // Lấy tất cả IP address của host
                var hostName = Dns.GetHostName();
                var addresses = Dns.GetHostAddresses(hostName);

                // Ưu tiên IPv4
                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }

                // Nếu không có IPv4, lấy IPv6
                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        return address.ToString();
                    }
                }

                return "Unable to determine IP";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}