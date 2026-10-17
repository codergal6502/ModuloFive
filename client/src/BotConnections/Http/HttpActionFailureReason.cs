
namespace ModuloFivePadClient.BotConnections.Http
{
    public class HttpActionFailureReason : IActionFailureReason
    {
        private HttpRequestException? _ex;

        public HttpActionFailureReason(HttpRequestException ex)
        {
            this._ex = ex;
            FailureReason = "";
        }

        public HttpActionFailureReason(HttpResponseMessage responseMessage)
        {
            FailureReason = $"HTTP {responseMessage.StatusCode}: {responseMessage.ReasonPhrase}";
        }

        public string FailureReason { get; }

        public HttpRequestException? Exception { get => _ex;}
    }
}