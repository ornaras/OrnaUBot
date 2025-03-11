using OrnaUBot.Exceptions;
using System.Diagnostics;
using System.Net.Http.Headers;

internal static class Utils
{
    public static async Task<string?> SendPostRequest(string url, string json, string msgErr)
    {
        try
        {
            using var http = new HttpClient();
            var content = new StringContent(json, new MediaTypeHeaderValue("application/json"));
            var resp = await http.PostAsync(url, content);
            var respContent = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) 
                throw new HttpResponseException(resp.StatusCode, respContent);
            return null;
        }
        catch(Exception e)
        {
            ShowError($"{e}");
            return msgErr;
        }
    }
    
    public static async Task<string?> SendPostRequest(string url, MultipartFormDataContent content, string msgErr)
    {
        try
        {
            using var http = new HttpClient();
            var resp = await http.PostAsync(url, content);
            var respContent = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpResponseException(resp.StatusCode, respContent);
            return null;
        }
        catch(Exception e)
        {
            ShowError($"{e}");
            return msgErr;
        }
    }

    public static string? StartProcess(string path, string args, string msgErr)
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
        ShowError(error.ReadToEnd());
        return msgErr;
    }

    public static string? CopyAllFiles(string srcDir, string destDir, string[] ignoreExts, string msgErr)
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
        catch(Exception e)
        {

            ShowError($"{e}");
            return msgErr;
        }
    }

    public static void ShowError(string error)
    {
        var defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ForegroundColor = defaultColor;
    }
}
