using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    public class DoiMayBayService
    {

        // API đổi máy bay
        private const string API_PLANE =
            "https://ux7ir7zqt1.execute-api.ap-southeast-1.amazonaws.com/post/plane";

        private static readonly HttpClient http = new HttpClient();

        private const int PLANE_MAX_SIZE = 96;

        private class PlaneResponse
        {
            public int planeIndex { get; set; }
            public string key { get; set; }
            public string downloadUrl { get; set; }
        }

       
        public async Task<(Image? Image, string? Key)> DoiMayBayAsync(int planeIndex)
        {
            var bodyObj = new { plane = planeIndex };
            string json = JsonSerializer.Serialize(bodyObj);

            using var resp = await http.PostAsync(
                API_PLANE,
                new StringContent(json, Encoding.UTF8, "application/json"));

            // Nếu lỗi thì ném message rõ ràng hơn
            if (!resp.IsSuccessStatusCode)
            {
                string errBody = await resp.Content.ReadAsStringAsync();
                throw new Exception(
                    $"API đổi máy bay lỗi {(int)resp.StatusCode} {resp.StatusCode}: {errBody}");
            }

            string text = await resp.Content.ReadAsStringAsync();

            var opt = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var data = JsonSerializer.Deserialize<PlaneResponse>(text, opt);

            if (data == null || string.IsNullOrEmpty(data.downloadUrl))
                return (null, null);

            // tải ảnh gốc từ presigned URL
            byte[] bytes = await http.GetByteArrayAsync(data.downloadUrl);
            using var ms = new MemoryStream(bytes);
            using var original = Image.FromStream(ms);

            //  Resize ảnh máy bay xuống nhỏ 
            Image resized = ResizePlaneImage(original, PLANE_MAX_SIZE);

            return (resized, data.key);
        }

        
        private static Image ResizePlaneImage(Image srcImage, int maxSize)
        {
            int w = srcImage.Width;
            int h = srcImage.Height;

            // Tính tỉ lệ scale để không chiều nào > maxSize
            float scale = (float)maxSize / Math.Max(w, h);
            if (scale > 1f) scale = 1f; // không phóng to ảnh nhỏ

            int newW = (int)(w * scale);
            int newH = (int)(h * scale);

            var destImage = new Bitmap(newW, newH);

            using (var g = Graphics.FromImage(destImage))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                g.DrawImage(srcImage, 0, 0, newW, newH);
            }

            return destImage;
        }

       
        internal async Task<Image?> GetImageAsync(string planeSkin)
        {
            throw new NotImplementedException();
        }
    }
}
