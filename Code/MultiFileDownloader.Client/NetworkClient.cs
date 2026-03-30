using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiFileDownloader.Client
{
    public class NetworkClient
    {
        // NÂNG CẤP: Thêm cơ chế xử lý lỗi (Exception Handling)
        public async Task<List<string>> GetFileList()
        {
            try
            {
                // Giả lập thời gian phản hồi từ Server (500ms - 1500ms)
                Random rnd = new Random();
                await Task.Delay(rnd.Next(500, 1500));

                // Giả lập trường hợp lỗi kết nối (ví dụ: 10% khả năng lỗi)
                // if (rnd.Next(1, 10) == 1) throw new Exception("Không thể kết nối tới Server!");

                return new List<string>
                {
                    "📄 Project_Final_v2.pdf",
                    "🎬 Introduction_Clip.mp4",
                    "📦 Resource_Pack.zip",
                    "🎵 Background_Theme.wav",
                    "📝 Readme_Instruction.txt"
                };
            }
            catch (Exception ex)
            {
                // Truyền lỗi ra ngoài để UI hiển thị thông báo đẹp cho người dùng
                throw new Exception("Lỗi danh sách: " + ex.Message);
            }
        }
    }
}