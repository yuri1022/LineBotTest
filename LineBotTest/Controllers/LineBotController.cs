using Microsoft.AspNetCore.Mvc;
using DotNetLineBotSdk.Message;
using DotNetLineBotSdk.MessageEvent;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System;

namespace LineBotTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineBotController : ControllerBase
    {
        private readonly string ChannelAccessToken = "5WON6edrZcCxGgHPkh+9H7Izo2a/kblCzimkAU/OUpiXleAFQDe8zASuO/xK+ivWRUQjna+moemhr00hkD7BTY/IJyWx03TwkL3oHObxyKtwKwEB+ZLe3RJKAKSXZ0hwkgcdghdsGF+vrLPV5bk4EQdB04t89/1O/w1cDnyilFU=";

        // POST api/<LineBotController>/webhook
        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] dynamic requestBody)
        {
            var events = JsonConvert.DeserializeObject<dynamic>(requestBody.ToString());
            System.Console.WriteLine(events.events);

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

                                if (userMessage.ToLower().Contains("flex"))
                                {
                                    await ReplyFlexMessage(replyToken);
                                }
                                else if (userMessage.Contains("笑話"))
                                {
                                    await ReplyJoke(replyToken);
                                }

                                else if (userMessage.Contains("天氣"))
                                {
                                    await ReplyWeather(replyToken);
                                }
                                else if (userMessage.ToLower().Contains("quickreply"))
                                {
                                    await ReplyWithQuickReply(replyToken);
                                }
                                else if (userMessage.Contains("location"))
                                {
                                    await RequestLocation(replyToken);
                                }
                                else if (userMessage.Contains("carousel"))
                                {
                                    await ReplyCarouselMessage(replyToken); 
                                }

                                else
                                {
                                    await ReplyMessage(replyToken, $"您說了：{userMessage}");
                                }
                                break;

                            case "sticker":
                                await ReplyStickerMessage(replyToken);
                                break;
                        }
                        break;

                    case "postback":
                        string postbackData = ev.postback.data.ToString();
                        if (postbackData == "action=pb")
                        {
                            await ReplyMessage(replyToken, "Postback 可以透過 webhook 呼叫後端程式，並且不會在聊天視窗留下訊息，可以使用 data 屬性附帶資料。");
                        }
                        else if (postbackData.StartsWith("action=select_datetime"))
                        {
                            string selectedDateTime = ev.postback.@params.datetime;
                            await ReplyMessage(replyToken, $"你選擇的時間是：{selectedDateTime}");
                        }
                        break;

                    case "location":
                        double latitude = ev.message.latitude;
                        double longitude = ev.message.longitude;
                        await ReplyMessage(replyToken, $"你分享的位置是：\n緯度：{latitude}\n經度：{longitude}");
                        break;
                    case "follow":
                        // 當 BOT 被加入時回應
                        await ReplyMessage(replyToken, "嗨！我是你的 LINE Bot，請輸入指令來與我互動。");
                        break;

                    case "unfollow":
                        // 當 BOT 被封鎖時回應
                        Console.WriteLine("Bot 已被封鎖QQ");
                        break;
                    case "join":
                        // 當 BOT 被加入群組時回應
                        await ReplyMessage(replyToken, "大家好！我是你的 LINE Bot，請輸入指令來與我互動。");
                        break;

                    case "leave":
                        // 當 BOT 被移出群組時
                        Console.WriteLine("Bot 已被移出群組");
                        break;

                    default:
                        Console.WriteLine($"未處理的事件類型：{ev.type}");
                        break;
                }
            }

            return Ok(events.events);
        }


        // 一般文字訊息回覆
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
                    System.Console.WriteLine("回覆成功");
                }
                else
                {
                    System.Console.WriteLine($"回覆失敗：{response.StatusCode}");
                }
            }
        }

        private async Task ReplyStickerMessage(string replyToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

                var stickerMessage = new
                {
                    type = "sticker",
                    packageId = "1",
                    stickerId = "1"
                };

                var jsonContent = new
                {
                    replyToken = replyToken,
                    messages = new[] { stickerMessage }
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.line.me/v2/bot/message/reply", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("貼圖回覆成功");
                }
                else
                {
                    Console.WriteLine($"貼圖回覆失敗：{response.StatusCode}");
                }
            }
        }


        private async Task ReplyFlexMessage(string replyToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

                // Flex Message 的 JSON 結構
                var flexMessage = new
                {
                    type = "flex",
                    altText = "這是一個 Flex Message",
                    contents = new
                    {
                        type = "bubble",
                        hero = new
                        {
                            type = "image",
                            url = "https://developers-resource.landpress.line.me/fx/img/01_1_cafe.png", // 替換成你的圖片 URL
                            size = "full",
                            aspectRatio = "20:13",
                            aspectMode = "cover"
                        },
                        body = new
                        {
                            type = "box",
                            layout = "vertical",
                            contents = new object[]
                            {
                        new
                        {
                            type = "text",
                            text = "Hello, Flex Message!",
                            weight = "bold",
                            size = "xl"
                        },
                        new
                        {
                            type = "text",
                            text = "這是一個範例 Flex Message 回覆",
                            margin = "md",
                            size = "sm"
                        }
                            }
                        },
                        footer = new
                        {
                            type = "box",
                            layout = "vertical",
                            spacing = "md",
                            contents = new object[]
                            {
                        new
                        {
                            type = "button",
                            action = new
                            {
                                type = "message",
                                label = "回應類型 MESSAGE",
                                text = "message 類型的按鈕會直接在聊天室內顯示使用者輸入的內容"
                            },
                            style = "primary",
                            color = "#1DB446"
                        },
                        new
                        {
                            type = "button",
                            action = new
                            {
                                type = "postback",
                                label = "回應類型 POSTBACK",
                                data = "action=pb"
                            },
                            style = "primary",
                            color = "#000000"
                        },
                        new
                        {
                            type = "button",
                            action = new
                            {
                                type = "datetimepicker",
                                label = "選擇日期時間",
                                data = "action=select_datetime",
                                mode = "datetime",
                                initial = "2025-03-18T12:00",
                                max = "2030-12-31T23:59",
                                min = "2023-01-01T00:00"
                            },
                            style = "primary",
                            color = "#FF5733"
                        },
                            }
                        }
                    }
                };

                var jsonContent = new
                {
                    replyToken = replyToken,
                    messages = new[] { flexMessage }
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.line.me/v2/bot/message/reply", content);

                if (response.IsSuccessStatusCode)
                {
                    System.Console.WriteLine("Flex Message 回覆成功");
                }
                else
                {
                    System.Console.WriteLine($"Flex Message 回覆失敗：{response.StatusCode}");
                }
            }
        }

        private async Task RequestLocation(string replyToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

                var locationMessage = new
                {
                    type = "template",
                    altText = "請分享您的位置",
                    template = new
                    {
                        type = "buttons",
                        text = "請點擊下方按鈕分享您的位置",
                        actions = new object[]
                        {
                    new
                    {
                        type = "location",
                        label = "分享位置"
                    }
                        }
                    }
                };

                var jsonContent = new
                {
                    replyToken = replyToken,
                    messages = new[] { locationMessage }
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.line.me/v2/bot/message/reply", content);

                if (response.IsSuccessStatusCode)
                {
                    System.Console.WriteLine("位置請求訊息發送成功");
                }
                else
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    System.Console.WriteLine($"位置請求訊息發送失敗：{response.StatusCode} - {responseText}");
                }
            }
        }
        private async Task ReplyCarouselMessage(string replyToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

                // Carousel Message 的 JSON 結構
                var carouselMessage = new
                {
                    type = "template",
                    altText = "這是輪播模板",
                    template = new
                    {
                        type = "carousel",
                        columns = new object[]
                        {
                    new
                    {
                        thumbnailImageUrl = "https://i.pinimg.com/736x/2d/95/e5/2d95e5886fc4c65a6778b5fee94a7d59.jpg", // 替換成您的圖片 URL
                        title = "Item 1",
                        text = "Description of Item 1",
                        actions = new object[]
                        {
                            new
                            {
                                type = "message",
                                label = "Buy",
                                text = "Buy Item 1"
                            }
                        }
                    },
                    new
                    {
                        thumbnailImageUrl = "https://images.unsplash.com/photo-1514228742587-6b1558fcca3d?fm=jpg&q=60&w=3000&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Mnx8Y3VwfGVufDB8fDB8fHww", // 替換成您的圖片 URL
                        title = "Item 2",
                        text = "Description of Item 2",
                        actions = new object[]
                        {
                            new
                            {
                                type = "uri",
                                label = "Go to website",
                                uri = "https://google.com"
                            }
                        }
                    }
                        }
                    }
                };

                var jsonContent = new
                {
                    replyToken = replyToken,
                    messages = new[] { carouselMessage }
                };

                var content = new StringContent(JsonConvert.SerializeObject(jsonContent), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.line.me/v2/bot/message/reply", content);

                if (response.IsSuccessStatusCode)
                {
                    System.Console.WriteLine("Carousel Message 回覆成功");
                }
                else
                {
                    System.Console.WriteLine($"Carousel Message 回覆失敗：{response.StatusCode}");
                }
            }
        }

        private async Task ReplyWithQuickReply(string replyToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChannelAccessToken);

                var quickReplyItems = new object[]  // <-- 明確指定為 object[]
                {
            new
            {
                type = "action",
                action = new
                {
                    type = "uri",
                    label = "打開網站",
                    uri = "https://www.google.com"
                }
            },
            new
            {
                type = "action",
                action = new
                {
                    type = "camera",
                    label = "開啟相機"
                }
            },
            new
            {
                type = "action",
                action = new
                {
                    type = "cameraRoll",
                    label = "選擇相簿照片"
                }
            }
                };

                var quickReplyMessage = new
                {
                    replyToken = replyToken,
                    messages = new[]
                    {
                new
                {
                    type = "text",
                    text = "請選擇以下選項：",
                    quickReply = new
                    {
                        items = quickReplyItems  // <-- 使用定義好的陣列
                    }
                }
            }
                };

                var content = new StringContent(JsonConvert.SerializeObject(quickReplyMessage), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.line.me/v2/bot/message/reply", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Quick Reply 回覆成功");
                }
                else
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Quick Reply 回覆失敗：{response.StatusCode} - {responseText}");
                }
            }
        }


        private async Task ReplyWeather(string replyToken)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=25.033&longitude=121.5654&current_weather=true";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        dynamic weatherData = JsonConvert.DeserializeObject(result);

                        double temperature = weatherData.current_weather.temperature;
                        string weatherMessage = $"🌤️ 當前氣溫：{temperature}°C";

                        await ReplyMessage(replyToken, weatherMessage);
                    }
                    else
                    {
                        await ReplyMessage(replyToken, "抱歉，無法取得天氣資訊。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("API 呼叫失敗：" + ex.Message);
                    await ReplyMessage(replyToken, "發生錯誤，請稍後再試。");
                }
            }
        }

        private async Task ReplyJoke(string replyToken)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "https://v2.jokeapi.dev/joke/Any?type=single";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        dynamic jokeData = JsonConvert.DeserializeObject(result);

                        string joke = jokeData.joke;
                        await ReplyMessage(replyToken, $"😂 {joke}");
                    }
                    else
                    {
                        await ReplyMessage(replyToken, "抱歉，暫時無法取得笑話。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("API 呼叫失敗：" + ex.Message);
                    await ReplyMessage(replyToken, "發生錯誤，請稍後再試。");
                }
            }
        }

    }
}
