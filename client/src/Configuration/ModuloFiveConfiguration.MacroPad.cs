namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public class MacroPad
        {
            public Guid? ProductId { get; set; } = null;
            public Version? Version { get; set; } = null;
            public Guid? SerialId { get; set; } = null;
            public string? Nickname { get; set; } = null;
            public List<Switch> Switches { get; set; } = [];
        }
    }
}