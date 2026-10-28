using ModuloFivePadClient.BotConnections;

namespace MockObjects;

public class MockBotConnectionBuilder : IBotConnectionBuilder
{
    public IList<IBotConnection> ActiveBotConnections { get; set; } = [];

    public event BotConnectedHandler? BotConnected;

    public Task<IList<IBotConnection>> BuildAllAsync(CancellationToken? cancellation = null)
    {
        throw new NotImplementedException();
    }

    public void DoBotConnected(MockBotConnection bConn)
    {
        ActiveBotConnections.Add(bConn);
        BotConnected?.Invoke(this, bConn);
    }
}
