using System;
using System.Collections.Generic;
using System.Text;
using System.Net;


namespace MultiFileDownloader.Shared
{
    // Packet format: [Command:1byte][Length:4bytes big-endian][Payload:Nbytes]
    public static class PacketHelper
    {
        // Build a complete packet (header + payload)
        public static byte[] CreatePacket(Command cmd, byte[] payload)
        {
            int len = payload.Length;

            byte[] header = new byte[5];

            header[0] = (byte)cmd;

            byte[] lengthBytes =
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(len));

            Array.Copy(lengthBytes, 0, header, 1, 4);

            return header.Concat(payload).ToArray();
        }

        // Extract payload length from 5-byte header
        public static int ParseLength(byte[] header)
        {
            int len = BitConverter.ToInt32(header, 1);

            return IPAddress.NetworkToHostOrder(len);
        }
    }
}