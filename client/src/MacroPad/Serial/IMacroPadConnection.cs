namespace ModuloFivePadClient.MacroPad.Serial
{
    public delegate void SwitchStateChangedEvent(IMacroPadConnection macroPad, SwitchStateChangeMessage message);
    
    public interface IMacroPadConnection : IDisposable
    {
        Guid ProductId { get; }
        Version MacroPadVersion { get; }
        Guid SerialId { get; }
        public event SwitchStateChangedEvent? SwitchStateChanged;
    }
}