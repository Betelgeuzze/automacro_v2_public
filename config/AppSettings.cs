using System.Text.Json;
using automacro.models;

namespace automacro.config
{
    public static class AppSettings
    {
        private static string ConfigFile => Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        private static AppConfig _config;

        public static AppConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = LoadConfig();
                }
                return _config;
            }
        }

        private static AppConfig LoadConfig()
        {
            if (!File.Exists(ConfigFile))
            {
                // Removed debug log
                var defaultConfig = new AppConfig();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(ConfigFile);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var config = JsonSerializer.Deserialize<AppConfig>(json, options);
                // Removed debug log
                return config ?? new AppConfig();
            }
            catch (Exception ex)
            {
                // Removed debug log
                return new AppConfig();
            }
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                File.WriteAllText(ConfigFile, json);
                _config = config;
                // Removed debug log
            }
            catch (Exception ex)
            {
                // Removed debug log
            }
        }
    }
}
