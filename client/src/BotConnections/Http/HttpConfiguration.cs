namespace ModuloFivePadClient.BotConnections.Http
{
    public class HttpConfiguration
    {
        public string? Name { get; set; }
        public string? BaseUri { get; set; }
        public string? StatusRelativeUrl{ get; set; }
        public string? StatusTransform { get; set; }
        public string? ActionListRelativeUrl { get; set; }
        public string? ActionListTransform { get; set; }
        public string? ActionInvocationRelativeUrlFormatString { get; set; }
        public string? ActionInvocationRequestTransform { get; set; }
        public string? ActionInvocationResponseTransform { get; set; }
    }
}