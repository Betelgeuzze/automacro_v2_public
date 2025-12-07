using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace automacro.models
{
    public class TelegramBot
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TelegramBot(TelegramConfig config)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _baseUrl = $"https://api.telegram.org/bot{config.BotToken}";
        }

        public async Task SendGameNotRunningAsync(TelegramConfig telegram, MessagesConfig messages)
        {
            if (!telegram.Enabled || string.IsNullOrEmpty(telegram.ChatId)) return;

            var message = messages.GameNotRunning;
            await SendMessageAsync(telegram.ChatId, message);
        }

        public async Task SendPopupDetectedAsync(TelegramConfig telegram, MessagesConfig messages)
        {
            if (!telegram.Enabled || string.IsNullOrEmpty(telegram.ChatId)) return;

            var message = messages.PopupDetected;
            await SendMessageAsync(telegram.ChatId, message);
        }

        public async Task SendUiCheckFailedAsync(TelegramConfig telegram, MessagesConfig messages)
        {
            if (!telegram.Enabled || string.IsNullOrEmpty(telegram.ChatId)) return;

            var message = messages.UiCheckFailed;
            await SendMessageAsync(telegram.ChatId, message);
        }

        private async Task SendMessageAsync(string chatId, string text)
        {
            try
            {
                var payload = new
                {
                    chat_id = chatId,
                    text = text,
                    parse_mode = "HTML"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/sendMessage", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Removed debug log
                }
                else
                {
                    // Removed debug log
                }
            }
            catch (Exception ex)
            {
                // Removed debug log
            }
        }
    }
}
