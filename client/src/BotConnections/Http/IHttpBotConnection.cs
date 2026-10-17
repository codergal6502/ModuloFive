namespace ModuloFivePadClient.BotConnections.Http
{
    public interface IHttpBotConnection : IBotConnection
    {
        public string? BaseUri { get; }
    }
}