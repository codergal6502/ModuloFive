namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class Bot
        {
            public string? Name { get; set; }
            public string? Nickname { get; set; }
            public BotAction[]? BotActions { get; set; } = [];
            public bool IsEnabled { get; set; }
        }
    }
}