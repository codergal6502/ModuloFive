using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.MacroPad.Serial;

namespace ModuloFivePadClient.RunnerCore
{
    public class RunnerLinkage
    {
        private SwitchState ToSerialSwitchState(ModuloFiveConfiguration.SwitchState s)
        {
            switch (s)
            {
                case ModuloFiveConfiguration.SwitchState.On:
                    return SwitchState.On;
                case ModuloFiveConfiguration.SwitchState.Off:
                    return SwitchState.Off;
                case ModuloFiveConfiguration.SwitchState.Changed:
                    return SwitchState.Changed;
                default:
                    return SwitchState.Unknown;
            }
        }
        public RunnerLinkage(ModuloFiveConfiguration.Linkage cfgLinkage)
        {
            ModuloFiveConfiguration.SwitchState? triggerState = cfgLinkage.TriggerState;

            if (triggerState == null)
            {
                this.Invalid = true;
                return;
            }

            this.Invalid = false;
            this.MacroPadSerialId = cfgLinkage.MacroPadSerialId;
            this.SwitchNumber = cfgLinkage.SwitchNumber;
            this.TriggerState = ToSerialSwitchState((ModuloFiveConfiguration.SwitchState)triggerState);
            this.BotName = cfgLinkage.BotName;
            this.BotActionId = cfgLinkage.BotActionId;
        }

        public Guid? MacroPadSerialId { get; set; }
        public int? SwitchNumber { get; set; }
        public SwitchState? TriggerState { get; set; }
        public string? BotName { get; set; }
        public string? BotActionId { get; set; }
        public List<RunnerLinkageArgument> Arguments { get; set; } = [];
        public bool Invalid { get; }
    }
}