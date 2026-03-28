using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace MultiFileDownloader.Shared
{
    public static class PacketHelper
    {
        public static byte[] CreatePacket(Command cmd, byte[] payload)
        {
            int length = payload.Length;

            int networkLength = IPAddress.HostToNetworkOrder(length);

            byte[] packet = new byte[5 + length];

            packet[0] = (byte)cmd;

            Array.Copy(BitConverter.GetBytes(networkLength), 0, packet, 1, 4);

            Array.Copy(payload, 0, packet, 5, payload.Length);

            return packet;
        }

        public static int ParseLength(byte[] header)
        {
            int networkLength = BitConverter.ToInt32(header, 1);

            return IPAddress.NetworkToHostOrder(networkLength);
        }
    }
}
