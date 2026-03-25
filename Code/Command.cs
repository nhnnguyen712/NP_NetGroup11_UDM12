public enum Command : byte
{
    RequestFileList = 1,
    SendFileList = 2,
    RequestDownload = 3,
    SendFile = 4,
    FileChunk = 5,
    DownloadComplete = 6
}