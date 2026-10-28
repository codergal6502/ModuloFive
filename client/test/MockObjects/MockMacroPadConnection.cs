using ModuloFivePadClient.MacroPad.Serial;

namespace MockObjects;

public class MockMacroPadConnection : IMacroPadConnection
{
    public Guid ProductId { get; set; }
    public Version MacroPadVersion { get; set; } = new Version(0, 0, 0);
    public Guid SerialId { get; set; }

    public event SwitchStateChangedEvent? SwitchStateChanged;

    public void DoSwitchStateChanged(IMacroPadConnection macroPad, SwitchStateChangeMessage message)
    {
        SwitchStateChanged?.Invoke(macroPad, message);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
