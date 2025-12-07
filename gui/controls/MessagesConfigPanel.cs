using System;
using System.Drawing;
using System.Windows.Forms;
using automacro.models;

namespace automacro.gui.Controls
{
    public class MessagesConfigPanel : UserControl
    {
        private TextBox txtGameNotRunning;
        private TextBox txtPopupDetected;
        private TextBox txtUiCheckFailed;

        private MessagesConfig _messagesConfig;

        // Removed unused events

        public MessagesConfigPanel()
        {
            SetupControls();
        }

        private void SetupControls()
        {
            this.SuspendLayout();

            var grpMessages = new GroupBox
            {
                Text = "System Messages",
                Location = new Point(10, 10),
                Size = new Size(430, 200)
            };

            var lblGameNotRunning = new Label { Text = "Game Not Running:", Location = new Point(15, 25), Size = new Size(160, 23) };
            txtGameNotRunning = new TextBox { Location = new Point(15, 50), Size = new Size(370, 23) };

            var lblPopupDetected = new Label { Text = "Popup Detected:", Location = new Point(15, 80), Size = new Size(120, 23) };
            txtPopupDetected = new TextBox { Location = new Point(15, 105), Size = new Size(370, 23) };

            var lblUiCheckFailed = new Label { Text = "UI Check Failed:", Location = new Point(15, 135), Size = new Size(120, 23) };
            txtUiCheckFailed = new TextBox { Location = new Point(15, 160), Size = new Size(370, 23) };

            grpMessages.Controls.AddRange(new Control[] {
                lblGameNotRunning, txtGameNotRunning,
                lblPopupDetected, txtPopupDetected,
                lblUiCheckFailed, txtUiCheckFailed
            });

            this.Controls.Add(grpMessages);
            this.Size = new Size(420, 200);
            this.ResumeLayout(false);
        }

        public void LoadMessagesConfig(MessagesConfig config)
        {
            _messagesConfig = config ?? new MessagesConfig();
            txtGameNotRunning.Text = _messagesConfig.GameNotRunning ?? "";
            txtPopupDetected.Text = _messagesConfig.PopupDetected ?? "";
            txtUiCheckFailed.Text = _messagesConfig.UiCheckFailed ?? "";
        }

        public MessagesConfig GetMessagesConfig()
        {
            if (_messagesConfig == null)
                _messagesConfig = new MessagesConfig();

            _messagesConfig.GameNotRunning = txtGameNotRunning?.Text ?? "";
            _messagesConfig.PopupDetected = txtPopupDetected?.Text ?? "";
            _messagesConfig.UiCheckFailed = txtUiCheckFailed?.Text ?? "";
            return _messagesConfig;
        }
    }
}
