namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class Switch
        {
            public int? SwitchNumber { get; set; }
            public string? Nickname { get; set; }
            public SwitchMode? SwitchMode { get; set; }
        }
    }
}