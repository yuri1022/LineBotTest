using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace LineBotTest.Services
{
    public static class Utility
    {
        public static async Task ReplyMessageAsync(string accessToken, string replyToken, string message)
        {
            var httpClient = new HttpClient();
            var url = "https://api.line.me/v2/bot/message/reply";

            var requestBody = new
            {
                replyToken = replyToken,
                messages = new[]
                {
                    new { type = "text", text = message }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
