using System;
using automacro.modules;

namespace automacro.modules
{
    public class TelegramKeySender : IDisposable
    {
        private readonly KeyPressService _keyPressService;

        public TelegramKeySender()
        {
            _keyPressService = new KeyPressService();
            _keyPressService.Start();
        }

        public void SendKey(string key, int duration = 100, IntPtr targetWindow = default)
        {
            if (targetWindow != IntPtr.Zero)
                _keyPressService.TargetWindowHandle = targetWindow;

            // Support syntax like "backspace 5" to send key 5 times
            string[] parts = key.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string keyName = parts[0];
            int repeat = 1;
            if (parts.Length == 2 && int.TryParse(parts[1], out int n) && n > 0)
                repeat = n;

            string mappedKey = keyName switch
            {
                "esc" or "ESC" or "escape" or "ESCAPE" => "esc",
                "enter" or "ENTER" or "return" or "RETURN" => "enter",
                "tab" or "TAB" => "tab",
                "space" or "SPACE" => "space",
                "backspace" or "BACKSPACE" => "backspace",
                "delete" or "DELETE" => "delete",
                "up" or "UP" => "up",
                "down" or "DOWN" => "down",
                "left" or "LEFT" => "left",
                "right" or "RIGHT" => "right",
                _ => keyName
            };

            for (int i = 0; i < repeat; i++)
            {
                // For single letter, preserve case and send Shift+key for uppercase
                if (keyName.Length == 1 && char.IsLetter(keyName[0]))
                {
                    char c = keyName[0];
                    if (char.IsUpper(c))
                    {
                        // Press Shift down
                        _keyPressService.QueueKeyDown("shift");
                        // Press the lowercase letter
                        _keyPressService.QueueKeyPress(new KeyPressAction
                        {
                            Id = "TELEGRAM",
                            Key = char.ToLower(c).ToString(),
                            Duration = duration,
                            DueTime = DateTime.Now
                        });
                        // Release Shift
                        _keyPressService.QueueKeyUp("shift");
                    }
                    else
                    {
                        _keyPressService.QueueKeyPress(new KeyPressAction
                        {
                            Id = "TELEGRAM",
                            Key = c.ToString(),
                            Duration = duration,
                            DueTime = DateTime.Now
                        });
                    }
                }
                else
                {
                    _keyPressService.QueueKeyPress(new KeyPressAction
                    {
                        Id = "TELEGRAM",
                        Key = mappedKey,
                        Duration = duration,
                        DueTime = DateTime.Now
                    });
                }
            }
        }

        public void Dispose()
        {
            _keyPressService?.Dispose();
        }
    }
}
