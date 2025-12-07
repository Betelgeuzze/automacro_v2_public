using System;
using System.Collections.Generic;
using automacro.config;
using automacro.models;

namespace automacro.modules
{
    public class HotkeyScheduler
    {
        private readonly Dictionary<string, HotkeyState> _hotkeyStates;
        private const int HOTKEY_EXECUTION_BUFFER = 50;
        
        public event Action<string> OnLog;
        
        private class HotkeyState
        {
            public string Id { get; set; } = "";
            public HotkeyConfig Config { get; set; } = new HotkeyConfig();
            public DateTime LastRun { get; set; } = DateTime.MinValue;
            public DateTime NextRun { get; set; } = DateTime.MinValue;
            public bool IsDue => DateTime.Now >= NextRun;
        }

        public HotkeyScheduler()
        {
            _hotkeyStates = new Dictionary<string, HotkeyState>(7);
            Log("⏰ HotkeyScheduler initialized");
        }

        public void LoadHotkeys(AppConfig config)
        {
            _hotkeyStates.Clear();
            DateTime currentTime = DateTime.Now;
            
            if (config.Macro.Hotkeys != null)
            {
                foreach (var hotkey in config.Macro.Hotkeys)
                {
Log($"Loading hotkey {hotkey.Key}: Key='{hotkey.Value.Key}', Interval={hotkey.Value.Interval}s, Enabled={hotkey.Value.Enabled}");
                    
                    var state = new HotkeyState 
                    { 
                        Id = hotkey.Key, 
                        Config = hotkey.Value,
                        LastRun = currentTime,
NextRun = currentTime.AddMilliseconds(hotkey.Value.Interval * 1000 + HOTKEY_EXECUTION_BUFFER)
                    };
                    
                    _hotkeyStates[hotkey.Key] = state;
                }
            }
        }

        public List<KeyPressAction> GetDueHotkeys(DateTime currentTime)
        {
            var dueHotkeys = new List<KeyPressAction>(7);
            
            foreach (var state in _hotkeyStates.Values)
            {
                if (!state.Config.Enabled || string.IsNullOrWhiteSpace(state.Config.Key))
                    continue;
                
                if (state.IsDue)
                {
                    dueHotkeys.Add(new KeyPressAction
                    {
                        Id = state.Id,
                        Key = state.Config.Key,
                        Duration = state.Config.PressDuration,
                        DueTime = currentTime
                    });
                    
                    // Update for next run
                    state.LastRun = currentTime;
state.NextRun = currentTime.AddMilliseconds(state.Config.Interval * 1000 + HOTKEY_EXECUTION_BUFFER);
                }
            }
            
            return dueHotkeys;
        }

        public void UpdateHotkey(string hotkeyId, bool enabled)
        {
            if (_hotkeyStates.ContainsKey(hotkeyId))
            {
                _hotkeyStates[hotkeyId].Config.Enabled = enabled;
                Log($"Hotkey {hotkeyId} {(enabled ? "enabled" : "disabled")}");
            }
        }

        public void ResetAllTimers()
        {
            DateTime currentTime = DateTime.Now;
            foreach (var state in _hotkeyStates.Values)
            {
                if (state.Config.Enabled)
                {
                    state.LastRun = currentTime;
state.NextRun = currentTime.AddMilliseconds(state.Config.Interval * 1000 + HOTKEY_EXECUTION_BUFFER);
                }
            }
            Log("✅ Hotkey timers reset");
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
