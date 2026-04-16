using System.Net;
using System.Net.Sockets;

namespace MultiFileDownloader.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Lấy port từ: 
            // 1. Command line argument (ví dụ: dotnet run 9999)
            // 2. Environment variable (PORT=9999)
            // 3. Default: 8888
            int port = 8888;

            // Kiểm tra command line argument
            if (args.Length > 0 && int.TryParse(args[0], out int argPort))
            {
                port = argPort;
                Console.WriteLine($"[CONFIG] Port từ command line argument: {port}");
            }
            // Kiểm tra environment variable
            else if (int.TryParse(Environment.GetEnvironmentVariable("FILE_DOWNLOADER_PORT"), out int envPort))
            {
                port = envPort;
                Console.WriteLine($"[CONFIG] Port từ environment variable: {port}");
            }
            else
            {
                Console.WriteLine($"[CONFIG] Sử dụng port mặc định: {port}");
            }

            // Kiểm tra port có hợp lệ không
            if (port < 1 || port > 65535)
            {
                Console.WriteLine($"❌ Port không hợp lệ: {port}");
                Console.WriteLine($"⚠️  Port phải từ 1 đến 65535");
                return;
            }

            // Lấy IP address của server
            string serverIP = GetServerIP();

            // Bind tất cả IP addresses (0.0.0.0)
            TcpListener server = new TcpListener(IPAddress.Any, port);

            try
            {
                server.Start();
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine();
                Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                                                           ║");
                Console.WriteLine("║  ❌ PORT ALREADY IN USE                                   ║");
                Console.WriteLine("║                                                           ║");
                Console.WriteLine($"║  Port {port} đang bị chiếm bởi một ứng dụng khác!         ");
                Console.WriteLine("║                                                           ║");
                Console.WriteLine("║  💡 Cách giải quyết:                                      ║");
                Console.WriteLine("║                                                           ║");
                Console.WriteLine($"║  1️⃣  Chạy với port khác:                                ║");
                Console.WriteLine($"║     dotnet run 9999                                       ║");
                Console.WriteLine("║                                                           ║");
                Console.WriteLine($"║  2️⃣  Đặt environment variable:                           ║");
                Console.WriteLine($"║     SET FILE_DOWNLOADER_PORT=9999                        ║");
                Console.WriteLine("║                                                           ║");
                Console.WriteLine($"║  3️⃣  Tìm process chiếm port {port}:                      ");
                Console.WriteLine("║     netstat -ano | findstr :" + port);
                Console.WriteLine("║     taskkill /PID <PID> /F                                ║");
                Console.WriteLine("║                                                           ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi start server: {ex.Message}");
                return;
            }

            // In thông tin server đẹp
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║        🔗 Multi File Downloader Server                    ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("╠═══════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║  📍 Server IP (Localhost): 127.0.0.1:{port}".PadRight(60) + "║");
            Console.WriteLine($"║  📍 Server IP (Network):   {serverIP}:{port}".PadRight(60) + "║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  📋 Files Location: ./files                               ║");
            Console.WriteLine("║  ⏱️  Started at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(41) + "║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  ✅ Waiting for client connections...                     ║");
            Console.WriteLine("║                                                           ║");
            Console.WriteLine("║  💡 Để dừng server: Ấn Ctrl+C                             ║");
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
                var hostName = Dns.GetHostName();
                var addresses = Dns.GetHostAddresses(hostName);

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }

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