using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace automacro.models
{
    public class TelegramBot
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramBot(TelegramConfig config)
        {
            if (!string.IsNullOrWhiteSpace(config?.BotToken))
            {
                _botClient = new TelegramBotClient(config.BotToken);
            }
            else
            {
                _botClient = null;
            }
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
            if (_botClient == null) return;
            try
            {
                var request = new Telegram.Bot.Requests.SendMessageRequest
                {
                    ChatId = chatId,
                    Text = text,
                    ParseMode = ParseMode.Html
                };
                await _botClient.SendRequest(request, default);
            }
            catch (Exception)
            {
                // Removed debug log
            }
        }
    }
}
