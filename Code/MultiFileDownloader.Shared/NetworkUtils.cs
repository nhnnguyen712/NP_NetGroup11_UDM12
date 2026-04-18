using System.Net.Sockets;

namespace MultiFileDownloader.Shared
{
    public static class NetworkUtils
    {
        public static async Task<byte[]> ReadExactlyAsync(NetworkStream stream, int size)
        {
            byte[] buffer = new byte[size];
            int offset = 0;

            while (offset < size)
            {
                int read = await stream.ReadAsync(buffer, offset, size - offset);

                if (read == 0)
                    throw new Exception("Disconnected");

                offset += read;
            }

            return buffer;
        }
    }
}