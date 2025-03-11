using OrnaUBot.Extensions;
using OrnaUBot.Utils;
using System.Diagnostics;

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
        var installer_path = Path.Combine(Configuration.projectScankassPath, "ScanKassInstaller");
        var app_path = Path.Combine(installer_path, "app");
        var msg = CopyAllFiles(Configuration.smbScankassPath, app_path, ["zip","db"], "Не удалось скопировать файлы с сервера");
        if (msg is not null) return msg;
        #endregion

        #region Сборка инсталятора
        var args = $"{string.Join(' ', defines)} {Path.Combine(installer_path, "!setup.nsi")}";
        msg = StartProcess("makensis", args, "Не удалось собрать установщик");
        if (msg is not null) return msg;
        #endregion

        #region Перенос инсталятора
        var installer_filename = "ScanKassSetup.exe";
        File.Move(Path.Combine(installer_path, installer_filename), 
            Path.Combine(Configuration.smbLocalPath, installer_filename), true);
        #endregion

        #region Отправка тестировщику уведомления

        if (!isRemote)
        {
            var message = $"<b>ScanKassSetup был обновлен</b> и ожидает проверку по пути <code>{Configuration.smbLocalPath.Replace("\\", @"\\")}</code>";
            msg = await TelegramBot.SendMessage(Configuration.telegramTesterId, message, "Не удалось оповестить тестировщика");
        }
        else
        {
            var message = $"Сборка {DateTime.Now:yyyy'-'MM'-'dd' 'HH':'mm}";
            var filepath = Path.Combine(Configuration.smbLocalPath, installer_filename);
            msg = await TelegramBot.SendFile(Configuration.telegramTesterId, message, filepath, "Не удалось отправить дистрибутив");
        }

        #endregion

        return msg ?? "ОК";
    }

    private static string? CopyAllFiles(string srcDir, string destDir, string[] ignoreExts, string msgErr)
    {
        try
        {
            var files = Directory.GetFiles(srcDir);
            foreach (var file in files)
            {
                if (ignoreExts.Contains(file.Split('.')[^1])) continue;
                var filename = file.Split(Path.DirectorySeparatorChar)[^1];
                File.Copy(file, Path.Combine(destDir, filename), true);
            }
            return null;
        }
        catch (Exception e)
        {

            e.ShowConsole();
            return msgErr;
        }
    }


    private static string? StartProcess(string path, string args, string msgErr)
    {
        var info = new ProcessStartInfo(path, args)
        {
            Verb = "runas",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        var proc = Process.Start(info)!;
        proc.WaitForExit();
        using var error = proc.StandardError;
        if (error.EndOfStream) return null;
        new Exception(error.ReadToEnd()).ShowConsole();
        return msgErr;
    }
}
