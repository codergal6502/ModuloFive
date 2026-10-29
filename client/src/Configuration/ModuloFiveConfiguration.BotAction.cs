namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class BotAction
        {
            public string? Name { get; set; }
            public string? BotActionId { get; set; }
            public BotActionParameter[] Parameters { get; set; } = [];
        }
    }
}