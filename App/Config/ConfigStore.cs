using System.Text.Json;

namespace App.Config;

public class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true
    };
    
    public static string GetConfigPath()
    {
        // SpecialFolder.ApplicationData maps to ~/.config on linux & %AppData% on Windows
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Ostrich");

        Directory.CreateDirectory(dir);
        
        return Path.Combine(dir, "config.json");
    }
    
    /// <summary>
    /// Load saved configurations or use the default.
    /// </summary>
    public static AppConfig Load()
    {
        var path = GetConfigPath();
        if (!File.Exists(path)) return new AppConfig();

        try
        {
            var json = File.ReadAllText(path);
            
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOpts) ?? new AppConfig();
        }
        catch { return new AppConfig(); }
    }

    /// <summary>
    /// Store the user configuration.
    /// </summary>
    public static void Save(AppConfig cfg)
    {
        File.WriteAllText(GetConfigPath(), JsonSerializer.Serialize(cfg, JsonOpts));
    }
}