namespace ModuloFivePadClient.Configuration

{
    public interface IModuloFiveConfigurationFileSystemService
    {
        bool ConfigurationFileExists();
        ModuloFiveConfiguration ReadConfiguration();
        void WriteConfiguration(ModuloFiveConfiguration configuration);
    }
}