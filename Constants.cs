namespace OrnaUBot;

internal static class Constants
{
    public static string AppDataPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OrnaUBot");

    public static string ConfFile => Path.Combine(AppDataPath, "settings.json");
}
