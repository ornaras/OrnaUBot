using System.Text.Json.Nodes;

namespace OrnaUBot;

internal static class Configuration
{
#pragma warning disable IDE1006
    public static string smbScankassPath { get; private set; } = null!;
    public static string projectScankassPath { get; private set; } = null!;
    public static string telegramBotToken { get; private set; } = null!;
    public static string smbLocalPath { get; private set; } = null!;
    public static long telegramTesterId { get; private set; }
    public static long telegramMeId { get; private set; }
#pragma warning restore IDE1006

    public static void Init()
    {
        var path = Constants.ConfFile;
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "{}");
            throw new FileNotFoundException($"Файл конфигурации был создан по пути: {path}! Выполните настройку!");
        }
        var content = File.ReadAllText(path);
        var json = JsonNode.Parse(content)!;
        
        smbScankassPath = json[nameof(smbScankassPath)]!.GetValue<string>();
        projectScankassPath = json[nameof(projectScankassPath)]!.GetValue<string>();
        telegramBotToken = json[nameof(telegramBotToken)]!.GetValue<string>();
        smbLocalPath = json[nameof(smbLocalPath)]!.GetValue<string>();
        telegramTesterId = json[nameof(telegramTesterId)]!.GetValue<long>();
        telegramMeId = json[nameof(telegramMeId)]!.GetValue<long>();
    }
}