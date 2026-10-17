namespace ModuloFivePadClient.MacroPad.Serial
{
    public delegate void MacroPadConnectedHandler(IMacroPadConnectionBuilder sender, IMacroPadConnection macroPadConnection);

    public interface IMacroPadConnectionBuilder
    {
        Task<IEnumerable<IMacroPadConnection>> BuildAllAsync(CancellationToken? cancellationToken = null);
        IEnumerable<IMacroPadConnection> ActiveMacroPadConnections { get; }
        event MacroPadConnectedHandler? MacroPadConnected;
    }
}