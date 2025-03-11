using OrnaUBot.Exceptions;
using System.Net.Http.Headers;
using OrnaUBot.Extensions;

namespace OrnaUBot.Utils
{
    internal static class TelegramBot
    {
        private static string UrlApi => $"https://api.telegram.org/bot{Configuration.Telegram_Bot_Token}";

        /// <summary>
        /// Отправка личного сообщения в Telegram
        /// </summary>
        /// <param name="chatId">Идентификатор чата</param>
        /// <param name="msg">Текст личного сообщения</param>
        /// <param name="msgErr">Текст ошибки</param>
        /// <returns>Сообщение об ошибке</returns>
        public static async Task<string?> SendMessage(long chatId, string msg, string msgErr = "Не удалось отправить сообщение")
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(Configuration.Telegram_Bot_Token);
                var json = $"{{\"chat_id\":{chatId},\"text\":\"{msg}\",\"parse_mode\":\"HTML\"}}";
                using var http = new HttpClient();
                var content = new StringContent(json, new MediaTypeHeaderValue("application/json"));
                var resp = await http.PostAsync($"{UrlApi}/sendMessage", content);
                var respContent = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                    throw new HttpResponseException(resp.StatusCode, respContent);
                return null;
            }
            catch (Exception e)
            {
                e.ShowConsole();
                return msgErr;
            }
        }


        /// <summary>
        /// Отправка файла в Telegram
        /// </summary>
        /// <param name="chatId">Идентификатор чата</param>
        /// <param name="msg">Текст сообщения</param>
        /// <param name="filepath">Абсолютный путь к файлу</param>
        /// <param name="msgErr">Текст ошибки</param>
        /// <returns>Сообщение об ошибке</returns>
        public static async Task<string?> SendFile(long chatId, string msg, string filepath, string msgErr = "Не удалось отправить файл")
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(Configuration.Telegram_Bot_Token);
                using var stream = new StreamContent(File.OpenRead(filepath));
                var content = new MultipartFormDataContent
                {
                    { new StringContent($"{chatId}"), "chat_id" },
                    { new StringContent(msg), "caption" },
                    { new StringContent("HTML"), "parse_mode" },
                    { stream, "document", Path.GetFileName(filepath) }
                };
                using var http = new HttpClient();
                var resp = await http.PostAsync($"{UrlApi}/sendDocument", content);
                var respContent = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                    throw new HttpResponseException(resp.StatusCode, respContent);
                return null;
            }
            catch (Exception e)
            {
                e.ShowConsole();
                return msgErr;
            }
        }
    }
}
