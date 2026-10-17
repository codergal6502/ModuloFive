namespace ModuloFivePadClient.Configuration
{    
    public delegate void ConfigurationChangeEvent(ModuloFiveConfiguration configuration);
    public interface IModuloFivePadConfigurationProvider
    {
        ModuloFiveConfiguration CloneCurrentConfigurationCopy();
        void UpdateConfiguration(ModuloFiveConfiguration configuration, ConfigurationPersistenceMode configurationPersistenceMode = ConfigurationPersistenceMode.Immediate);

        void WriteConfiguration();

        ConfigurationChangeEvent? ConfigurationChanged { get; set; }

        TimeSpan WriteBufferRateLimit { get; set; }
    }
}