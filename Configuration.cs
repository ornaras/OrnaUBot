using System.Text.Json.Nodes;

namespace OrnaUBot;

internal static class Configuration
{
    public static string SMB_ScanKass_Path { get; private set; } = null!;
    public static string Project_ScanKass_Path { get; private set; } = null!;
    public static string Telegram_Bot_Token { get; private set; } = null!;
    public static string SMB_Local_Path { get; private set; } = null!;
    public static long Telegram_Tester_Id { get; private set; }
    
    internal static string AppDataPath => 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OrnaUBot");

    public static void Init()
    {
        if(!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);
        var path = Path.Combine(AppDataPath, "settings.json");
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "{}");
            throw new FileNotFoundException($"Файл конфигурации был создан по пути: {path}! Выполните настройку!");
        }
        var content = File.ReadAllText(path);
        var json = JsonNode.Parse(content)!;
        
        SMB_ScanKass_Path = json[FormatText(nameof(SMB_ScanKass_Path))]!.GetValue<string>();
        Project_ScanKass_Path = json[FormatText(nameof(Project_ScanKass_Path))]!.GetValue<string>();
        Telegram_Bot_Token = json[FormatText(nameof(Telegram_Bot_Token))]!.GetValue<string>();
        SMB_Local_Path = json[FormatText(nameof(SMB_Local_Path))]!.GetValue<string>();
        Telegram_Tester_Id = json[FormatText(nameof(Telegram_Tester_Id))]!.GetValue<long>();
    }

    private static string FormatText(string text)
    {
        text = text.ToLower();
        var res = "";
        for(var i = 0; i < text.Length; i++)
        {
            if (text[i] == '_')
            {
                i++;
                res += $"{text[i]}".ToUpper();
            } else res += text[i];
        }
        return res;
    }
}