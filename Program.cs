using System;
using System.Windows.Forms;
using automacro.models;

namespace automacro
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (sender, args) =>
            {
                MessageBox.Show(args.Exception.ToString(), "Unhandled UI Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                MessageBox.Show(args.ExceptionObject.ToString(), "Unhandled Domain Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            // Start TelegramCommandReceiver in background
            var configManager = new config.ConfigManager();
            var telegramConfig = configManager.LoadConfig().Telegram;
            var macroEngine = new automacro.modules.MacroEngine(configManager.LoadConfig());
            var receiver = new automacro.models.TelegramCommandReceiver(telegramConfig.BotToken, telegramConfig.ChatId, macroEngine);
            var receiverThread = new System.Threading.Thread(() => receiver.Start());
            receiverThread.IsBackground = true;
            receiverThread.Start();

            ApplicationConfiguration.Initialize();
            Application.Run(new gui.MainForm(receiver, macroEngine));
        }
    }
}
