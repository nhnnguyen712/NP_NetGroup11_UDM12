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

        SendFileSize = 4,
        SendFileChunk = 5,

        DownloadComplete = 6
    }
}