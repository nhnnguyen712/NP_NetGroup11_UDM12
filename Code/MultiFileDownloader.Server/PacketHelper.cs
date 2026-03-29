using System;
using System.Net;

public class PacketHelper
{
    public static int ParseLength(byte[] header)
    {
        byte[] lenBytes = new byte[4];
        Array.Copy(header, 1, lenBytes, 0, 4);

        int netLength = BitConverter.ToInt32(lenBytes, 0);
        return IPAddress.NetworkToHostOrder(netLength);
    }
}