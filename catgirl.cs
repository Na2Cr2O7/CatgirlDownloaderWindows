using CatGirlDownloaderWindows2;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CatgirlDownloader
{

    // 定义数据模型类
    public class ImageData
    {
        [JsonPropertyName("images")]
        public ImageInfo[] Images { get; set; }
    }

    public class ImageInfo
    {
        [JsonPropertyName("nsfw")]
        public bool Nsfw { get; set; }

        [JsonPropertyName("tags")]
        public string[]? Tags { get; set; }

        [JsonPropertyName("likes")]
        public int Likes { get; set; }

        [JsonPropertyName("favorites")]
        public int Favorites { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("originalHash")]
        public string? OriginalHash { get; set; }

        [JsonPropertyName("uploader")]
        public User? Uploader { get; set; }

        [JsonPropertyName("approver")]
        public User? Approver { get; set; }

        [JsonPropertyName("artist")]
        public string? Artist { get; set; }

        [JsonPropertyName("comments")]
        public Comment[]? Comments { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    public class Comment
    {
        // 如果未来 comments 有内容，可以扩展字段。
        // 当前为空数组，保留空类即可。
    }

    public class CatgirlDownloaderAPI
    {
        private readonly string endpoint = "https://nekos.moe/api/v1/random/image";
        public const string website = "https://nekos.moe/post/";
       public ImageData info; // 使用具体的数据模型替代 dynamic

        private static readonly HttpClient httpClient = new();

        public CatgirlDownloaderAPI()
        {
            httpClient.Timeout = TimeSpan.FromSeconds(10);
        }
        
        public async Task<string> GetRandomImageIdAsync(NSFWOption nsfwMode = NSFWOption.BLOCK_NSFW)
        {
            try
            {
                string url = endpoint;

                if (nsfwMode == NSFWOption.ONLY_NSFW)
                {
                    url += "?nsfw=true";
                }
                else if (nsfwMode == NSFWOption.BLOCK_NSFW)
                {
                    url += "?nsfw=false";
                }

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return "None";
                }

                string responseContent = await response.Content.ReadAsStringAsync();

                // 使用 System.Text.Json 反序列化
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                ImageData data = JsonSerializer.Deserialize<ImageData>(responseContent, options)!;
                this.info = data;

                if (data?.Images?.Length > 0)
                {
                    return data.Images[0].Id;
                }

                return "None";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "None";
            }
        }

        // 同步版本
        public string GetRandomImageId(NSFWOption nsfwMode = NSFWOption.BLOCK_NSFW)
        {
            try
            {
                string url = endpoint;

                if (nsfwMode == NSFWOption.ONLY_NSFW)
                {
                    url += "?nsfw=true";
                }
                else if (nsfwMode == NSFWOption.BLOCK_NSFW)
                {
                    url += "?nsfw=false";
                }

                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return "None";
                }

                string responseContent = response.Content.ReadAsStringAsync().Result;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                ImageData data = JsonSerializer.Deserialize<ImageData>(responseContent, options);
                this.info = data;

                if (data?.Images?.Length > 0)
                {
                    return data.Images[0].Id;
                }

                return "None";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "None";
            }
        }

        public async Task<string> GetImageUrlAsync(NSFWOption nsfwMode = NSFWOption.BLOCK_NSFW)
        {
            string imageId = await GetRandomImageIdAsync(nsfwMode);
            return "https://nekos.moe/image/" + imageId;
        }

        // 同步版本
        public string GetImageUrl(NSFWOption nsfwMode = NSFWOption.BLOCK_NSFW)
        {
            string imageId = GetRandomImageId(nsfwMode);
            return "https://nekos.moe/image/" + imageId;
        }

        public async Task<byte[]> GetImageAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    byte[] imageData = await client.GetByteArrayAsync(url);
                    return imageData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        // 同步版本
        public byte[]? GetImage(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    Task<byte[]> task = client.GetByteArrayAsync(url);
                    return task.Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}