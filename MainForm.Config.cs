// MainForm.Config.cs
using System;
using automacro.config;
using automacro.models;

namespace automacro.gui
{
    public partial class MainForm
    {
        private void OnConfigChanged(AppConfig config)
        {
            _config = config;
        }

        private void SaveConfigFromUI()
        {
            try
            {
                if (_configManager == null || _config == null)
                {
                    MessageBox.Show("Configuration is not initialized. Please select a process target and restart the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _config = configPanel.GetConfig();
                if (telegramConfigPanel != null)
                    _config.Telegram = telegramConfigPanel.GetTelegramConfig();
                if (messagesConfigPanel != null)
                    _config.Messages = messagesConfigPanel.GetMessagesConfig();
                bool saved = _configManager.SaveConfig(_config);

                if (saved)
                {
                    MessageBox.Show("✅ Configuration saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (consolePanel != null)
                        consolePanel.AppendToLog("✅ Configuration saved successfully");
                    if (_coordinator != null)
                        _coordinator.ReloadHotkeys();
                }
                else
                {
                    MessageBox.Show("❌ Failed to save configuration!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (consolePanel != null)
                        consolePanel.AppendToLog("❌ Failed to save configuration");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (consolePanel != null)
                    consolePanel.AppendToLog($"❌ Error saving config: {ex.Message}");
            }
        }
    }
}
