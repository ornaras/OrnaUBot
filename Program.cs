using OrnaUBot;
using OrnaUBot.Scenario;

do
{
    Configuration.Init();
    Console.Clear();

    #region Меню действий
    Console.WriteLine("1) Обновление инсталятора ScanKass");
    Console.WriteLine("2) Заказ обеда");
    Console.WriteLine();
    Console.WriteLine("0) Выход из приложения");
    #endregion

    Console.Write("Выберите действие: ");
    var input = Console.ReadLine();
    if (!int.TryParse(input, out var code)) continue;

    #region Действия
    var msg = code switch
    {
        0 => Exit(),
        1 => ScanKass.UpdateInstaller().Result,
        2 => Borshch18.SendOrder().Result
    };
    #endregion

    Console.WriteLine();
    Console.WriteLine($"Действие завершено: {msg}");
    Console.WriteLine("ENTER, для закрытия бота");
    Console.ReadLine();
    return;
} while (true);

static string Exit()
{
    Environment.Exit(0);
    return "";
}