using System;
using System.Threading.Tasks;

public class DownloadManager
{
    public async Task DownloadFile(string fileName,
        Action<int> progress,
        Action<string> speed)
    {
        for (int i = 0; i <= 100; i += 10)
        {
            await Task.Delay(200);

            progress(i);
            speed($"{i * 2} KB/s");
        }
    }
}