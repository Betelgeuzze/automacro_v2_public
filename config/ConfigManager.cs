using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using automacro.models;

namespace automacro.config
{
    public class ConfigManager
    {
        private string ConfigFile { get; set; }
        private AppConfig DefaultConfig { get; set; }

public ConfigManager(string configFile = "config.json")
{
    // Use current working directory for config file (project root in development)
    string baseDir = Directory.GetCurrentDirectory();
    ConfigFile = Path.Combine(baseDir, configFile);
    DefaultConfig = GetDefaultConfig();

    // Removed debug log
    // Removed debug log
    
    var directory = Path.GetDirectoryName(ConfigFile);
    // Removed debug log
}

        private bool IsDirectoryWritable(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath)) return false;
            
            try
            {
                string testFile = Path.Combine(directoryPath, "write_test.txt");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private AppConfig GetDefaultConfig()
        {
            var config = new AppConfig();

            // Set up default hotkeys (similar to your Python version)
config.Macro.Hotkeys = new Dictionary<string, HotkeyConfig>(7)
            {
                ["1"] = new HotkeyConfig { Key = "", Interval = 0, PressDuration = 0, Enabled = false },
                ["2"] = new HotkeyConfig { Key = "", Interval = 0, PressDuration = 0, Enabled = false },
                ["3"] = new HotkeyConfig { Key = "", Interval = 0, PressDuration = 0, Enabled = false },
                ["4"] = new HotkeyConfig { Key = "", Interval = 0, PressDuration = 0, Enabled = false },
                ["5"] = new HotkeyConfig { Key = "", Interval = 0, PressDuration = 0, Enabled = false },
                ["6"] = new HotkeyConfig { Key = "", Interval = 0, PressDuration = 0, Enabled = false },
                ["7"] = new HotkeyConfig { Key = "", Interval = 0, PressDuration = 0, Enabled = false }
            };

            // Default messages
            config.Messages = new MessagesConfig
            {
                GameNotRunning = "VM Game is not running",
                PopupDetected = "VM Popup detected in game",
                UiCheckFailed = "VM UI check failed - Game might be crashed"
            };

            return config;
        }

        public AppConfig LoadConfig()
        {
            if (!File.Exists(ConfigFile))
            {
                // Removed debug log
                CreateDefaultConfig();
            }

            try
            {
                string json = File.ReadAllText(ConfigFile);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var config = JsonSerializer.Deserialize<AppConfig>(json, options);
                
                // Merge with default config to ensure all keys exist
                var mergedConfig = MergeWithDefaults(config);
                // Removed debug log
                return mergedConfig;
            }
catch (Exception)
            {
                // Removed debug log
                return DefaultConfig;
            }
        }

        private void CreateDefaultConfig()
        {
            var directory = Path.GetDirectoryName(ConfigFile);
            if (string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            SaveConfig(DefaultConfig);
        }

        public bool SaveConfig(AppConfig config)
        {
            try
            {
                // Removed debug log
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(ConfigFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigFile, json);
                
                // Removed debug log
                return true;
            }
catch (Exception)
            {
                // Removed debug log
                // Removed debug log
                return false;
            }
        }

private AppConfig MergeWithDefaults(AppConfig userConfig)
        {
            if (userConfig == null)
                return DefaultConfig;

            // Top-level
            if (userConfig.MainLoop == null)
                userConfig.MainLoop = new MainLoopConfig();
            if (userConfig.Macro == null)
                userConfig.Macro = new MacroConfig();
            if (userConfig.TargetGame == null)
                userConfig.TargetGame = new TargetGameConfig();
            if (userConfig.Timing == null)
                userConfig.Timing = new TimingConfig();
            if (userConfig.Telegram == null)
                userConfig.Telegram = new TelegramConfig();
            if (userConfig.Paths == null)
                userConfig.Paths = new PathsConfig();
            if (userConfig.SearchRegions == null)
                userConfig.SearchRegions = new SearchRegionsConfig();
            if (userConfig.Messages == null)
                userConfig.Messages = new MessagesConfig();

            // MacroConfig sub-objects
            if (userConfig.Macro.Hotkeys == null)
                userConfig.Macro.Hotkeys = new Dictionary<string, HotkeyConfig>(7);

            // SearchRegionsConfig sub-objects
            if (userConfig.SearchRegions.Popup == null)
                userConfig.SearchRegions.Popup = new List<int>(4) { 600, 424, 891, 1057 };
            if (userConfig.SearchRegions.Ui == null)
                userConfig.SearchRegions.Ui = new List<int>(4) { 754, 1008, 941, 1114 };

            // Add more sub-object checks as needed

            return userConfig;
        }
    }
}
