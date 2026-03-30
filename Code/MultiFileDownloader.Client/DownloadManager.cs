    using System;
    using System.Threading.Tasks;
    using System.Threading;

    namespace MultiFileDownloader.Client
    {
        public class DownloadManager
        {
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
                    // Bắt đầu từ vị trí hiện tại (phục vụ Resume)
                    for (int i = startProgress; i <= 100; i++)
                    {
                        // Kiểm tra token liên tục để dừng ngay khi nhấn Pause
                        token.ThrowIfCancellationRequested();

                        // Giả lập thời gian tải (300ms mỗi 1%)
                        // Truyền token vào Task.Delay để dừng chờ ngay lập tức
                        await Task.Delay(200, token);

                        // Gửi dữ liệu về UI thông qua IProgress
                        progressReporter?.Report(i);

                        // Giả lập tốc độ tải biến thiên
                        double currentSpeed = rnd.NextDouble() * (5.5 - 1.2) + 1.2; // Từ 1.2 đến 5.5 MB/s
                        speedReporter?.Report(Math.Round(currentSpeed, 1));
                    }
                }
                catch (OperationCanceledException)
                {
                    // Khi nhấn Pause, token sẽ throw lỗi này. 
                    // Chúng ta catch ở đây để Task kết thúc trong êm đẹp.
                    speedReporter?.Report(0); // Reset tốc độ về 0 khi dừng
                }
                catch (Exception ex)
                {
                    // Các lỗi mạng khác nếu có
                    Console.WriteLine($"Lỗi tải file {fileName}: {ex.Message}");
                    throw;
                }
            }
        }
    }