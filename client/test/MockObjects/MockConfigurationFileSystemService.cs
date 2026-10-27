using ModuloFivePadClient.Configuration;

namespace MockObjects;

public class MockConfigurationFileSystemService : IModuloFiveConfigurationFileSystemService
{
    public Func<bool>? MockConfigurationFileExists { set; get; }

    public bool ConfigurationFileExists()
    {
        if (null == MockConfigurationFileExists)
        {
            throw new AssertFailedException($"Can't call {nameof(ConfigurationFileExists)} if {nameof(MockConfigurationFileExists)} is not set.");
        }
        else
        {
            return MockConfigurationFileExists();
        }
    }

    public Func<ModuloFiveConfiguration>? MockReadConfiguration { set; get; }

    public ModuloFiveConfiguration ReadConfiguration()
    {
        if (null == MockReadConfiguration)
        {
            throw new AssertFailedException($"Can't call {nameof(ReadConfiguration)} if {nameof(MockReadConfiguration)} is not set.");
        }
        else
        {
            return MockReadConfiguration();
        }
    }

    public Action<ModuloFiveConfiguration>? MockWriteConfiguration { get; set; }

    public void WriteConfiguration(ModuloFiveConfiguration configuration)
    {
        if (null == MockWriteConfiguration)
        {
            throw new AssertFailedException($"Can't call {nameof(WriteConfiguration)} if {nameof(MockWriteConfiguration)} is not set.");
        }
        else
        {
            MockWriteConfiguration(configuration);
        }
    }
}
