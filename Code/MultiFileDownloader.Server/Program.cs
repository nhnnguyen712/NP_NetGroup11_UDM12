using System.Net;
using System.Net.Sockets;

namespace MultiFileDownloader.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize FileService to ensure 'files' folder is created immediately on startup
            FileService.Initialize();

            // Determine port: CLI arg > env var > default 8888
            int port = 8888;

            if (args.Length > 0 && int.TryParse(args[0], out int argPort))
            {
                port = argPort;
                Console.WriteLine($"[CONFIG] Port from CLI argument: {port}");
            }
            else if (int.TryParse(Environment.GetEnvironmentVariable("FILE_DOWNLOADER_PORT"), out int envPort))
            {
                port = envPort;
                Console.WriteLine($"[CONFIG] Port from environment variable: {port}");
            }
            else
            {
                Console.WriteLine($"[CONFIG] Using default port: {port}");
            }

            // Validate port range
            if (port < 1 || port > 65535)
            {
                Console.WriteLine($"Invalid port: {port} (must be 1-65535)");
                return;
            }

            string serverIP = GetServerIP();

            // Bind on all interfaces (0.0.0.0)
            TcpListener server = new TcpListener(IPAddress.Any, port);

            try
            {
                server.Start();
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine();
                Console.WriteLine("PORT ALREADY IN USE");
                Console.WriteLine($"Port {port} is occupied by another application!");
                Console.WriteLine();
                Console.WriteLine("Solutions:");
                Console.WriteLine($"  1. Use a different port:        dotnet run 9999");
                Console.WriteLine($"  2. Set environment variable:    SET FILE_DOWNLOADER_PORT=9999");
                Console.WriteLine($"  3. Find process using port:     netstat -ano | findstr :{port}");
                Console.WriteLine($"     Then kill it:                taskkill /PID <PID> /F");
                Console.WriteLine();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
                return;
            }

            // Display startup banner
            Console.Clear();
            Console.WriteLine("  Multi File Downloader Server");
            Console.WriteLine("  ----------------------------");
            Console.WriteLine($"  Localhost : 127.0.0.1:{port}");
            Console.WriteLine($"  Network   : {serverIP}:{port}");
            Console.WriteLine($"  Files     : ./files");
            Console.WriteLine($"  Started   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Stop      : Ctrl+C");
            Console.WriteLine();

            // Accept client connections
            int clientCount = 0;

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clientCount++;

                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Client #{clientCount} connected from {clientIP}");

                // Handle each client on a separate task
                _ = Task.Run(() => ClientHandler.Handle(client));
            }
        }

        // Resolve the server's LAN IP address (prefer IPv4)
        static string GetServerIP()
        {
            try
            {
                var hostName = Dns.GetHostName();
                var addresses = Dns.GetHostAddresses(hostName);

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                        return address.ToString();
                }

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                        return address.ToString();
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