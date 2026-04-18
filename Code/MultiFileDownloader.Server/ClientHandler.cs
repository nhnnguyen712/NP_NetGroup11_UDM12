using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using MultiFileDownloader.Shared;

namespace MultiFileDownloader.Server
{
    // Handles a single client connection on a background task
    public static class ClientHandler
    {
        static string root = Path.GetFullPath("files");

        public static async Task Handle(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                while (true)
                {
                    // Read 5-byte packet header
                    byte[] header = await NetworkUtils.ReadExactlyAsync(stream, 5);

                    Command cmd = (Command)header[0];
                    int length = PacketHelper.ParseLength(header);
                    byte[] payload = await NetworkUtils.ReadExactlyAsync(stream, length);

                    // Dispatch command
                    switch (cmd)
                    {
                        case Command.RequestFileList:
                            await FileService.SendFileList(stream);
                            break;

                        case Command.RequestDownload:
                            string file = Encoding.UTF8.GetString(payload);
                            await FileService.SendFile(stream, file);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client disconnected: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}