namespace ModuloFivePadClient.Configuration
{
    public partial class ModuloFiveConfiguration
    {
        public ModuloFiveConfiguration() { }

        public ModuloFiveConfiguration(ModuloFiveConfiguration original)
        {
            IsFirstTimeUser = original.IsFirstTimeUser;
            ShowTechnicalInfo = original.ShowTechnicalInfo;
        }

        public bool IsFirstTimeUser { get; set; } = true;
        public List<MacroPad> MacroPads { get; set; } = [];
        public bool ShowTechnicalInfo { get; set; } = false;
        public List<Bot> Bots { get; set; } = [];
        public IEnumerable<Linkage> Linkages { get; set; } = [];
    }
}