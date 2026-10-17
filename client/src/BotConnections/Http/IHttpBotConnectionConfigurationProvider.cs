namespace ModuloFivePadClient.BotConnections.Http
{
    public interface IHttpBotConnectionConfigurationProvider
    {
        IEnumerable<HttpConfiguration> Configurations { get; }
    }
}