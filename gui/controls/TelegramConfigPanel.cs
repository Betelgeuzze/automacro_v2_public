using System;
using System.Drawing;
using System.Windows.Forms;
using automacro.models;

namespace automacro.gui.Controls
{
    public class TelegramConfigPanel : UserControl
    {
        private TextBox txtTelegramBotToken;
        private TextBox txtTelegramChatId;
        // Save button removed

        private TelegramConfig _telegramConfig;

        // Removed unused events

        public TelegramConfigPanel()
        {
            SetupControls();
        }

        private void SetupControls()
        {
            this.SuspendLayout();

            var grpTelegram = new GroupBox
            {
                Text = "Telegram Configuration",
                Location = new Point(10, 10),
                Size = new Size(430, 150)
            };

            var lblBotToken = new Label { Text = "Bot Token:", Location = new Point(15, 20), Size = new Size(100, 23) };
            txtTelegramBotToken = new TextBox { Location = new Point(15, 45), Size = new Size(400, 23) };
            var lblChatId = new Label { Text = "Chat ID:", Location = new Point(15, 80), Size = new Size(100, 23) };
            txtTelegramChatId = new TextBox { Location = new Point(15, 105), Size = new Size(400, 23) };

            grpTelegram.Controls.AddRange(new Control[] { lblBotToken, txtTelegramBotToken, lblChatId, txtTelegramChatId });

            this.Controls.Add(grpTelegram);

            this.Size = new Size(420, 190);
            this.ResumeLayout(false);
        }

        public void LoadTelegramConfig(TelegramConfig config)
        {
            _telegramConfig = config ?? new TelegramConfig();
            txtTelegramBotToken.Text = _telegramConfig.BotToken ?? "";
            txtTelegramChatId.Text = _telegramConfig.ChatId ?? "";
        }

        public TelegramConfig GetTelegramConfig()
        {
            if (_telegramConfig == null)
                _telegramConfig = new TelegramConfig();

            _telegramConfig.BotToken = txtTelegramBotToken?.Text ?? "";
            _telegramConfig.ChatId = txtTelegramChatId?.Text ?? "";
            return _telegramConfig;
        }

        // Save button and handler removed
    }
}
