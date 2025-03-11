using OrnaUBot.Utils;
using System.Text;
using System.Text.Json.Nodes;

namespace OrnaUBot.Scenario;

internal static class Borshch18
{
    public static string[] WhitelistCategories { get; } = 
        ["48", "49", "50", "51", "53", "57"];
    public static Dictionary<int, string> Categories { get; } = 
        new Dictionary<int, string>
    {
        { 48, "Салаты" },
        { 49, "Супы" },
        { 50, "Горячие блюда" },
        { 51, "Гарнир" },
        { 53, "Хлеб" },
        { 57, "Соусы" }
    };

    public static async Task<string> SendOrder()
    {
        var menu = await PullMenu();
        for (var i = 0; i < menu.Count; i++)
        {
            if (i == 0 || menu[i-1].Category != menu[i].Category)
            {
                (Console.BackgroundColor, Console.ForegroundColor) = 
                    (Console.ForegroundColor, Console.BackgroundColor);
                Console.WriteLine($"{Categories[menu[i].Category]}:");
            }
            Console.WriteLine($"{i + 1}) {menu[i]}");
        }
        Console.ResetColor();
        Console.WriteLine();
        Console.Write("Напиши номер блюд через пробел: ");
        var selectedIndex = Console.ReadLine()!.Split(' ').Where(t=>!string.IsNullOrWhiteSpace(t)).Select(t => int.Parse(t) - 1).ToArray();
        var res = new StringBuilder();
        var sum = 0;
        foreach (var index in selectedIndex)
        {
            res.AppendLine(menu[index].Name);
            sum += menu[index].Price;
        }
        res.Append($"<tg-spoiler>{sum}₽</tg-spoiler>");

        var msg = await TelegramBot.SendMessage(Configuration.Telegram_Me_Id, res.ToString());

        return msg ?? "OK";
    }

    private static async Task<List<Dish>> PullMenu()
    {
        using var http = new HttpClient();
        var req = new HttpRequestMessage(HttpMethod.Post, "https://borsh-panel.s2.sellkit.ru/NewApi/Content/GetCatalog.php")
        {
            Content = new MultipartFormDataContent
                {
                    {new StringContent("2"),"OrganisationID"},
                    {new StringContent("true"),"web"},
                }
        };
        req.Headers.Add("Authorization", "guest_token");
        var resp = await http.SendAsync(req);
        var respContent = await resp.Content.ReadAsStringAsync();
        var json = JsonNode.Parse(respContent)!;
        var res = new List<Dish>();

        foreach (var i in json["plates"]!.AsArray())
        {
            if (!WhitelistCategories.Contains(i!["categoryID"]!.GetValue<string>()))
                continue;
            res.Add(new Dish
            {
                Name = i!["name"]!.GetValue<string>(),
                Price = int.Parse(i!["values"]![0]!["price"]!.GetValue<string>()),
                Mass = i!["values"]![0]!["mass"]!.GetValue<string>(),
                Category = int.Parse(i!["categoryID"]!.GetValue<string>())
            });
        }

        res.Sort((p,n) => p.Category.CompareTo(n.Category));

        return res;
    }

    private class Dish
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public string Mass { get; set; }
        public int Category { get; set; }

        public override string ToString() => $"{Name} [{Price} руб \\ {Mass}]";
    }
}
