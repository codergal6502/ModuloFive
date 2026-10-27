namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class Linkage
        {
            public Guid? MacroPadSerialId { get; set; }
            public int? SwitchNumber { get; set; }
            public SwitchState? TriggerState { get; set; }
            public string? BotName { get; set; }
            public string? BotActionId { get; set; }
            public string? BotActionName { get; set; }
            public LinkageArgument[] Arguments { get; set; } = [];
        }
    }
}