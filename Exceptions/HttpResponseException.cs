using System.Net;

namespace OrnaUBot.Exceptions
{
    public class HttpResponseException(HttpStatusCode code, string content): 
        Exception($"На HTTP-запрос получен ответ с ошибкой:\nКод статуса: {(int)code}\nТело ответа:{content}");
}
