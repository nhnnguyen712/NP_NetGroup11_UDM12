using System;
using System.Collections.Generic;
using System.Text;

namespace MultiFileDownloader.Shared
{
    public class Packet
    {
        public Command Command { get; set; }

        public byte[] Payload { get; set; }
    }
}
