using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace plan_fighting_super_start
{
    public class S3ImageService
    {
        // API Gateway URL trỏ tới Lambda Image
        private const string API_IMAGE =
            "https://2cd28uutce.execute-api.ap-southeast-1.amazonaws.com/image";

        private static readonly HttpClient http = new HttpClient();

        // Cache ảnh đã tải
        private static readonly Dictionary<string, Image> _imageCache =
            new Dictionary<string, Image>();

        // Kích thước avatar nhỏ hơn để giảm lag
        private const int AVATAR_MAX_SIZE = 128;        // giảm từ 256 xuống
        private const long AVATAR_JPEG_QUALITY = 60L;   // JPEG quality thấp => dung lượng nhẹ

        // UPLOAD AVATAR: tự resize + nén JPEG rồi gửi lên AWS
        public async Task<string> UploadImageAsync(string filePath, string username)
        {
            byte[] optimizedBytes = OptimizeImage(filePath);

            string fileName = $"avatars/{username}.png";    // key trên S3
            string contentType = "image/jpeg";              // dữ liệu thực là JPEG

            string base64 = Convert.ToBase64String(optimizedBytes);

            var body = new
            {
                action = "upload",
                fileName = fileName,
                imageBase64 = base64,
                contentType = contentType
            };

            string json = JsonSerializer.Serialize(body);

            using var resp = await http.PostAsync(
                API_IMAGE,
                new StringContent(json, Encoding.UTF8, "application/json"));

            resp.EnsureSuccessStatusCode();

            string text = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);

            string key = doc.RootElement.GetProperty("key").GetString();

            // Clear cache cũ nếu có
            if (!string.IsNullOrEmpty(key) && _imageCache.ContainsKey(key))
            {
                _imageCache[key].Dispose();
                _imageCache.Remove(key);
            }

            return key;
        }

        // GET IMAGE TỪ S3 (CÓ CACHE)
        public async Task<Image> GetImageAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            // Nếu cache có thì trả luôn
            if (_imageCache.TryGetValue(key, out var cachedImg) && cachedImg != null)
            {
                return (Image)cachedImg.Clone();
            }

            // Gửi request lấy presigned URL
            var body = new
            {
                action = "getUrl",
                key = key
            };

            string json = JsonSerializer.Serialize(body);

            using var resp = await http.PostAsync(
                API_IMAGE,
                new StringContent(json, Encoding.UTF8, "application/json"));

            resp.EnsureSuccessStatusCode();

            string text = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);

            string url = doc.RootElement.GetProperty("downloadUrl").GetString();

            byte[] bytes = await http.GetByteArrayAsync(url);
            using var ms = new MemoryStream(bytes);
            var img = Image.FromStream(ms);

            // Lưu cache (clone)
            _imageCache[key] = (Image)img.Clone();

            return img;
        }

        // TỐI ƯU ẢNH (RESIZE + NÉN JPEG)
        private byte[] OptimizeImage(string filePath)
        {
            using var original = Image.FromFile(filePath);

            int w = original.Width;
            int h = original.Height;

            // scale nhỏ lại theo max-size
            float scale = (float)AVATAR_MAX_SIZE / Math.Max(w, h);
            if (scale > 1f) scale = 1f;

            int newW = (int)(w * scale);
            int newH = (int)(h * scale);

            using var bmp = new Bitmap(newW, newH);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.SmoothingMode = SmoothingMode.HighSpeed;

                g.DrawImage(original, 0, 0, newW, newH);
            }

            using var msOut = new MemoryStream();

            // JPEG Encoder
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            if (jpgEncoder != null)
            {
                EncoderParameters encoderParams = new EncoderParameters(1);

                EncoderParameter qualityParam =
                    new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,
                                         AVATAR_JPEG_QUALITY);

                encoderParams.Param[0] = qualityParam;

                bmp.Save(msOut, jpgEncoder, encoderParams);
            }
            else
            {
                // fallback
                bmp.Save(msOut, ImageFormat.Jpeg);
            }

            return msOut.ToArray();
        }

        // Tìm encoder JPEG
        private static ImageCodecInfo GetEncoder(ImageFormat format)    
        {
            var codecs = ImageCodecInfo.GetImageDecoders();

            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }

            return null;
        }
    }
}
