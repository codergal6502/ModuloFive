using ModuloFivePadClient.RunnerCore;

namespace MockObjects;

public class MockSwitchToggleWrapper : ISwitchToggleModeWrapper
{
    public event LogicalSwitchStateChangedEvent? SwitchStateChanged;

    public void DoSwitchStateChanged(MockMacroPadConnection macroPad, LogicalSwitchStateChangedArgs args)
    {
        SwitchStateChanged?.Invoke(macroPad, args);
    }
}
