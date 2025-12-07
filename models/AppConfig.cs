using System.Collections.Generic;

namespace automacro.models
{
    public class AppConfig
    {
        // Persisted region selection for automation
        public int RegionX { get; set; }
        public int RegionY { get; set; }
        public int RegionWidth { get; set; }
        public int RegionHeight { get; set; }
        public MainLoopConfig MainLoop { get; set; } = new MainLoopConfig();
        public MacroConfig Macro { get; set; } = new MacroConfig();
        public TargetGameConfig TargetGame { get; set; } = new TargetGameConfig();
        public TimingConfig Timing { get; set; } = new TimingConfig();
        public TelegramConfig Telegram { get; set; } = new TelegramConfig();
        public PathsConfig Paths { get; set; } = new PathsConfig();
        public SearchRegionsConfig SearchRegions { get; set; } = new SearchRegionsConfig();
        public MessagesConfig Messages { get; set; } = new MessagesConfig();

        // Mouse coordinate for game window
        public int MouseX { get; set; }
        public int MouseY { get; set; }
    }

    public class MessagesConfig
    {
        public string GameNotRunning { get; set; } = "VM Game is not running";
        public string PopupDetected { get; set; } = "VM Popup detected in game";
        public string UiCheckFailed { get; set; } = "VM UI check failed - Game might be crashed";
    }
    
    public class MainLoopConfig
    {
        public string Key { get; set; } = "";
/// <summary>
/// Interval between hotkey presses, in seconds.
/// </summary>
public int Interval { get; set; } = 1;
        public int PressDuration { get; set; } = 100;
    }
    
    public class MacroConfig
    {
        public Dictionary<string, HotkeyConfig> Hotkeys { get; set; } = new Dictionary<string, HotkeyConfig>(7);
        public int MainLoopInterval { get; set; } = 1000;
        public string MainLoopKey { get; set; } = "";
        public int MainKeyPressDuration { get; set; } = 100;
    }
    
    public class HotkeyConfig
    {
        public string Key { get; set; } = "";
        public int Interval { get; set; } = 1000;
        public int PressDuration { get; set; } = 100;
        public bool Enabled { get; set; } = true;
    }
    
    public class TargetGameConfig
    {
        public string Exe { get; set; } = "MapleStory.exe";
        public string Name { get; set; } = "Ranmelle";
    }
    
    public class TimingConfig
    {
        public int DetectInterval { get; set; } = 1000;
        public int DetectCooldown { get; set; } = 30;
        public int UiCheckCooldown { get; set; } = 60;
    }
    
    public class TelegramConfig
    {
        public string BotToken { get; set; }
        public string ChatId { get; set; }
        public bool Enabled { get; set; } = true;
    }
    
    public class PathsConfig
    {
        public string ResourcesDir { get; set; } = "resources";
        public string TemplateFile { get; set; } = "popup.png";
        public string UiCheckFile { get; set; } = "ui_check.png";
    }
    
    public class SearchRegionsConfig
    {
        public List<int> Popup { get; set; } = new List<int>(4) { 600, 424, 891, 1057 };
        public List<int> Ui { get; set; } = new List<int>(4) { 754, 1008, 941, 1114 };
    }
}
