using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;

namespace automacro.models
{

    public class TelegramCommandReceiver
    {
    private TelegramBotClient _botClient;
        private readonly string _chatId;
        private Automacro.Models.ProcessTarget _selectedProcess;
        private automacro.modules.MacroEngine _macroEngine;
        private automacro.modules.TelegramKeySender _telegramKeySender;


    public TelegramCommandReceiver(string botToken, string chatId, automacro.modules.MacroEngine macroEngine = null)
    {
        _botClient = null;
        _chatId = chatId;
        _selectedProcess = null;
        _macroEngine = macroEngine;
        _telegramKeySender = new automacro.modules.TelegramKeySender();
    }

    public void InitializeBotClient(string botToken)
    {
        if (!string.IsNullOrEmpty(botToken))
            _botClient = new TelegramBotClient(botToken);
        // No error or exception if token is null or empty
    }

    private DateTime _startTime;

    public void SetProcessTarget(Automacro.Models.ProcessTarget target)
    {
        _selectedProcess = target;
    }

    public void Start()
    {
        _startTime = DateTime.UtcNow;

        var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        if (_botClient != null)
        {
            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );
        }

        // No need for Console.ReadLine() in WinForms; keep running until app exits
        Application.ApplicationExit += (s, e) => cts.Cancel();
    }

    // Removed SelectProcess; process should be set externally

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Text != null)
        {
            var message = update.Message;
            // Ignore messages sent before the bot started
            if (message.Date.ToUniversalTime() < _startTime)
                return;

            var text = message.Text.Trim();
            if (text.ToLower() == "/screenshot")
            {
                // Take screenshot and send back from memory, no disk write
                if (_botClient != null)
                {
                    using (var stream = TakeScreenshotToStream())
                    {
                        var photoRequest = new Telegram.Bot.Requests.SendPhotoRequest
                        {
                            ChatId = message.Chat.Id,
                            Photo = stream,
                            Caption = "Here is your screenshot"
                        };
                        await bot.SendRequest(photoRequest, cancellationToken);
                    }
                }
            }
            else if (text.ToLower().StartsWith("/sendkey"))
            {
                var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var key = parts[1].Trim();
                    IntPtr windowHandle = _selectedProcess?.WindowHandle ?? IntPtr.Zero;
                    _telegramKeySender.SendKey(key, 100, windowHandle);
                    var textRequest = new Telegram.Bot.Requests.SendMessageRequest
                    {
                        ChatId = message.Chat.Id,
                        Text = $"Sent key: {key}"
                    };
                    if (_botClient != null)
                        await bot.SendRequest(textRequest, cancellationToken);
                }
                else
                {
                    var textRequest = new Telegram.Bot.Requests.SendMessageRequest
                    {
                        ChatId = message.Chat.Id,
                        Text = "Usage: /sendkey <key>"
                    };
                    await bot.SendRequest(textRequest, cancellationToken);
                }
            }
            else if (text.ToLower().StartsWith("/sendtext"))
            {
                var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var sendText = parts[1];
                    IntPtr windowHandle = _selectedProcess?.WindowHandle ?? IntPtr.Zero;
                    foreach (char c in sendText)
                    {
                        _telegramKeySender.SendKey(c.ToString(), 100, windowHandle);
                        Thread.Sleep(80); // Small delay between characters
                    }
                    var textRequest = new Telegram.Bot.Requests.SendMessageRequest
                    {
                        ChatId = message.Chat.Id,
                        Text = $"Sent text: {sendText}"
                    };
                    await bot.SendRequest(textRequest, cancellationToken);
                }
                else
                {
                    var textRequest = new Telegram.Bot.Requests.SendMessageRequest
                    {
                        ChatId = message.Chat.Id,
                        Text = "Usage: /sendtext <text>"
                    };
                    await bot.SendRequest(textRequest, cancellationToken);
                }
            }
            else
            {
                var textRequest = new Telegram.Bot.Requests.SendMessageRequest
                {
                    ChatId = message.Chat.Id,
                    Text = "Unknown command. Try /screenshot or /sendkey <key>"
                };
                await bot.SendRequest(textRequest, cancellationToken);
            }
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        // Error reporting removed
        return Task.CompletedTask;
    }

    // Screenshot method: captures region from target window to memory stream
    private System.IO.MemoryStream TakeScreenshotToStream()
    {
        if (_selectedProcess == null || _selectedProcess.ProcessId == 0)
            throw new Exception("No process selected for screenshot.");

        var process = automacro.utils.WindowUtils.GetGameProcess(_selectedProcess);
        if (process == null || process.MainWindowHandle == IntPtr.Zero)
            throw new Exception("Target window not found.");

        var region = new System.Drawing.Rectangle(582, 430, 766, 276);
        var bitmap = automacro.utils.WindowUtils.CaptureRegion(process.MainWindowHandle, region);

        if (bitmap == null)
            throw new Exception("Failed to capture screenshot.");

        var ms = new System.IO.MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        bitmap.Dispose();
        ms.Position = 0;
        return ms;
    }
}}
