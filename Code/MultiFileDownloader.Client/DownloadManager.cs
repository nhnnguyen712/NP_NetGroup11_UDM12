using System;
using System.Threading.Tasks;
using System.Threading;

// Class xử lý logic download (giả lập download)
public class DownloadManager
{
    // Dùng chung Random (tránh trùng seed)
    private static readonly Random rnd = new Random();

    public async Task DownloadFile(
        string fileName,
        IProgress<int> progressReporter,
        IProgress<double> speedReporter,
        CancellationToken token,
        int startProgress = 0)
    {
        try
        {
            // Loop từ progress hiện tại → 100
            for (int i = startProgress; i <= 100; i += 5)
            {
                // Nếu bị cancel → dừng ngay
                token.ThrowIfCancellationRequested();

                // Delay có token → pause mượt
                await Task.Delay(300, token);

                // Update progress
                progressReporter?.Report(i);

                // Update speed
                double currentSpeed = rnd.Next(100, 500);
                speedReporter?.Report(currentSpeed);
            }
        }
        catch (TaskCanceledException)
        {
            // Bị pause → thoát êm, không crash
            return;
        }
    }
}