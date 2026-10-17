using ModuloFivePadClient.MacroPad.Serial;
using ModuloFivePadClient.MacroPad.Serial.Messages;

namespace ModuloFivePadClient.RunnerCore
{
    public class LogicalSwitchStateChangedArgs(SwitchState logicalSwitchState, bool logicalSwitchStateChanged, SwitchStateChangeMessage originalMessage)
    {
        private readonly SwitchState _logicalSwitchState = logicalSwitchState;
        private readonly bool _logicalSwitchStateChanged = logicalSwitchStateChanged;
        private readonly SwitchStateChangeMessage _originalMessage = originalMessage;

        public SwitchState LogicalSwitchState { get => _logicalSwitchState; }
        public bool LogicalSwitchStateChanged { get => _logicalSwitchStateChanged; }
        public SwitchStateChangeMessage OriginalMessage { get => _originalMessage; }
    }

    public delegate void LogicalSwitchStateChangedEvent(IMacroPadConnection macroPad, LogicalSwitchStateChangedArgs args);

    public interface ISwitchToggleModeWrapper
    {
        event LogicalSwitchStateChangedEvent? SwitchStateChanged;
    }
}
