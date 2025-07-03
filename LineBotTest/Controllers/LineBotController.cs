using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace LineBotTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineBotController : ControllerBase
    {
        private readonly string ChannelAccessToken = "5WON6edrZcCxGgHPkh+9H7Izo2a/kblCzimkAU/OUpiXleAFQDe8zASuO/xK+ivWRUQjna+moemhr00hkD7BTY/IJyWx03TwkL3oHObxyKtwKwEB+ZLe3RJKAKSXZ0hwkgcdghdsGF+vrLPV5bk4EQdB04t89/1O/w1cDnyilFU=";

        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] dynamic requestBody)
        {
            var events = JsonConvert.DeserializeObject<dynamic>(requestBody.ToString());
            foreach (var ev in events.events)
            {
                string replyToken = ev.replyToken;

                switch ((string)ev.type)
                {
                    case "message":
                        switch ((string)ev.message.type)
                        {
                            case "text":
                                string userMessage = ev.message.text;

                                if (userMessage.Contains("減塑蔬行"))
                                {
                                    await ReplyFlexMessageFromFile(replyToken, "recycleEntry.json");
                                }
                                else if (userMessage.Contains("我要借杯子"))
                                {
                                    await ReplyFlexMessageFromFile(replyToken, "cupRental.json");
                                }
                                else if (userMessage.Contains("附近據點"))
                                {
                                    await ReplyFlexMessageFromFile(replyToken, "nearbyLocations.json");
                                }
                                else if (userMessage.Contains("我的租借紀錄"))
                                {
                                    await ReplyFlexMessageFromFile(replyToken, "rentalRecords.json");
                                }
                                else
                                {
                                    await ReplyMessage(replyToken, $"您說了：{userMessage}");
                                }
                                break;
                        }
                        break;

                    case "postback":
                        string data = ev.postback.data;
                        if (data == "nearby")
                        {
                            await ReplyFlexMessageFromFile(replyToken, "nearbyLocations.json");
                        }
                        else if (data == "records")
                        {
                            await ReplyFlexMessageFromFile(replyToken, "rentalRecords.json");
                        }
                        break;
                }
            }
            return Ok();
        }

        private string LoadFlexJson(string filename)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "FlexMessages", filename);
            return System.IO.File.ReadAllText(path);
        }

        private async Task ReplyFlexMessageFromFile(string replyToken, string filename)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

                string flexJsonString = LoadFlexJson(filename);
                dynamic flexContent = JsonConvert.DeserializeObject(flexJsonString);

                var jsonContent = new
                {
                    replyToken = replyToken,
                    messages = new[]
                    {
                        new
                        {
                            type = "flex",
                            altText = "Flex Message",
                            contents = flexContent
                        }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.line.me/v2/bot/message/reply", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Flex Message 回覆成功");
                }
                else
                {
                    Console.WriteLine("Flex Message 回覆失敗：" + response.StatusCode);
                }
            }
        }

        private async Task ReplyMessage(string replyToken, string message)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

                var jsonContent = new
                {
                    replyToken = replyToken,
                    messages = new[] { new { type = "text", text = message } }
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.line.me/v2/bot/message/reply", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("回覆成功");
                }
                else
                {
                    Console.WriteLine("回覆失敗：" + response.StatusCode);
                }
            }
        }
    }
}
