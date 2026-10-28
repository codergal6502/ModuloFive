namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class Linkage
        {
            public required Guid MacroPadSerialId { get; set; }
            public required int SwitchNumber { get; set; }
            public required SwitchState TriggerState { get; set; }
            public required string BotName { get; set; }
            public required string BotActionId { get; set; }
            public required string BotActionName { get; set; }
            public required LinkageArgument[] Arguments { get; set; }
        }
    }
}