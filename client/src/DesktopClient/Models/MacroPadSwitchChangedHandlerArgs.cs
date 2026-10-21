using System;
using OutboundSwitchState = ModuloFivePadClient.DesktopClient.ViewModels.SwitchState;

namespace ModuloFivePadClient.DesktopClient.Models
{
    public class MacroPadSwitchChangedHandlerArgs
    {
        public Guid? MacroPadSerialId { get; internal set; }
        public OutboundSwitchState SwitchState { get; internal set; }
        public int SwitchNumberOneBased { get; internal set; }
    }
}