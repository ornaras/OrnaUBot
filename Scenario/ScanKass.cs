namespace OrnaUBot.Scenario;

internal static class ScanKass
{
    public static async Task<string> UpdateInstaller()
    {
        Console.WriteLine();
        Console.WriteLine("Возможные параметры:");
        Console.WriteLine("Workflow - флаг на добавление планировщика");
        Console.WriteLine("Test - флаг на сборку для тестов");
        Console.WriteLine("Email - строка адреса электронной почты для автоматической авторизации");
        Console.Write("Параметры сборки инсталятора: ");
        var defines = Console.ReadLine()!.Split(' ').Where(a=>!string.IsNullOrWhiteSpace(a)).Select(a => $"/D{a}");
        
        Console.WriteLine();
        Console.WriteLine("1) Дистрибутив");
        Console.WriteLine("2) Уведомление");
        Console.Write("Выберите вариант отправки сообщения тестировщику: ");
        var isRemote = false;
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.KeyChar is not ('1' or '2')) continue;
            isRemote = key.KeyChar == '1';
            break;

        } while (true);

        #region Выгрузка с SMB
        var installer_path = Path.Combine(Configuration.Project_ScanKass_Path, "ScanKassInstaller");
        var app_path = Path.Combine(installer_path, "app");
        var msg = Utils.CopyAllFiles(Configuration.SMB_ScanKass_Path, app_path, ["zip","db"], "Не удалось скопировать файлы с сервера");
        if (msg is not null) return msg;
        #endregion

        #region Сборка инсталятора
        var args = $"{string.Join(' ', defines)} {Path.Combine(installer_path, "!setup.nsi")}";
        msg = Utils.StartProcess("makensis", args, "Не удалось собрать установщик");
        if (msg is not null) return msg;
        #endregion

        #region Перенос инсталятора
        var installer_filename = "ScanKassSetup.exe";
        File.Move(Path.Combine(installer_path, installer_filename), 
            Path.Combine(Configuration.SMB_Local_Path, installer_filename), true);
        #endregion

        #region Отправка тестировщику уведомления

        if (!isRemote)
        {
            var url = $"https://api.telegram.org/bot{Configuration.Telegram_Bot_Token}/sendMessage";
            var body =
                $"{{" +
                $"\"chat_id\":{Configuration.Telegram_Tester_Id}," +
                $"\"text\":\"<b>ScanKassSetup был обновлен</b> и ожидает проверку по пути <code>{Configuration.SMB_Local_Path.Replace("\\", @"\\")}</code>\"," +
                $"\"parse_mode\":\"HTML\"" +
                $"}}";
            msg = await Utils.SendPostRequest(url, body, "Не удалось оповестить тестировщика");
        }
        else
        {
            var url = $"https://api.telegram.org/bot{Configuration.Telegram_Bot_Token}/sendDocument";
            var content = new MultipartFormDataContent();
            content.Add(new StringContent($"Сборка {DateTime.Now:yyyy'-'MM'-'dd' 'HH':'mm}"), "caption");
            using (var stream = new StreamContent(File.OpenRead(Path.Combine(Configuration.SMB_Local_Path, installer_filename))))
                content.Add(stream, "document", installer_filename);
            msg = await Utils.SendPostRequest(url, content, "Не удалось отправить дистрибутив");
        }

        #endregion

        return msg ?? "ОК";
    }
}
