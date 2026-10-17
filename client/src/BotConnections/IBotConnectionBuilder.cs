namespace ModuloFivePadClient.BotConnections
{
    public delegate void BotConnectedHandler(IBotConnectionBuilder sender, IBotConnection botConnection);
    public interface IBotConnectionBuilder
    {
        public IList<IBotConnection> ActiveBotConnections { get; }
        event BotConnectedHandler? BotConnected;

        Task<IList<IBotConnection>> BuildAllAsync(CancellationToken? cancellation = null);
    }
}
