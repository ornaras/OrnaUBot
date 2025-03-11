namespace OrnaUBot.Extensions
{
    internal static class ExceptionExtensions
    {
        public static void ShowConsole(this Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{e}");
            Console.ResetColor();
        }
    }
}
