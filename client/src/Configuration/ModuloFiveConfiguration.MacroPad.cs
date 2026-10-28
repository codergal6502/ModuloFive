namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class MacroPad
        {
            public required Guid ProductId { get; set; }
            public required Version Version { get; set; }
            public required Guid SerialId { get; set; }
            public required string Nickname { get; set; }
            public required List<Switch> Switches { get; set; }
        }
    }
}