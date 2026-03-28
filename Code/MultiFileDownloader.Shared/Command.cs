using System;
using System.Collections.Generic;
using System.Text;

namespace MultiFileDownloader.Shared
{
    public enum Command : byte
    {
        RequestFileList = 1,
        SendFileList = 2,
        RequestDownload = 3,
        SendFileChunk = 4,
        DownloadComplete = 5,
        Error = 6
    }
}