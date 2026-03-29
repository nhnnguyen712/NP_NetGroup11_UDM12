using System;
using System.Net.Sockets;
using System.Threading.Tasks;

public class NetworkUtils
{
    public static async Task<byte[]> ReadExactlyAsync(NetworkStream stream, int count)
    {
        byte[] buffer = new byte[count];
        int offset = 0;

        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer, offset, count - offset);

            if (read == 0)
                throw new Exception("Disconnected");

            offset += read;
        }

        return buffer;
    }
}