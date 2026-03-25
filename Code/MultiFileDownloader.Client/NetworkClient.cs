using System.Collections.Generic;
using System.Threading.Tasks;

public class NetworkClient
{
    public async Task<List<string>> GetFileList()
    {
        await Task.Delay(500);

        return new List<string>
        {
            "file1.zip",
            "file2.mp4",
            "file3.pdf"
        };
    }
}