namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class Switch
        {
            public required int SwitchNumber { get; set; }
            public required string Nickname { get; set; }
            public required SwitchMode SwitchMode { get; set; }
        }
    }
}