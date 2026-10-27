using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModuloFivePadClient.Configuration.Json;
using ModuloFivePadClient.Utilities;
using static System.Environment;

namespace ModuloFivePadClient.Configuration
{
    public class ModuloFiveConfigurationFileSystemService : IModuloFiveConfigurationFileSystemService
    {
        private readonly ILogger<ModuloFiveConfigurationFileSystemService> _logger;
        private readonly PathAndFileNameSet _pathAndFileNameSet;

        public class PathAndFileNameSet(string preferencesDirectory, string configurationFileName)
        {
            public static PathAndFileNameSet Default { get; } = new PathAndFileNameSet(
                preferencesDirectory: EnvironmentUtilities.EnsureConfigurationDirectory(),
                configurationFileName: "settings.json"
            );

            public string PreferencesDirectory { get; } = preferencesDirectory;
            public string ConfigurationFileName { get; } = configurationFileName;
            public string ConfigurationFilePath { get => Path.Join(PreferencesDirectory, ConfigurationFileName); }
            public string ConfigurationWriteToFileName { get => $"{ConfigurationFileName}.temp"; }
            public string ConfigurationWriteToFilePath { get => Path.Join(PreferencesDirectory, ConfigurationWriteToFileName); }
        }

        public ModuloFiveConfigurationFileSystemService(ILogger<ModuloFiveConfigurationFileSystemService> logger, PathAndFileNameSet pathAndFileNameSet)
        {
            _logger = logger;
            _pathAndFileNameSet = pathAndFileNameSet;
        }

        public bool ConfigurationFileExists()
        {
            return File.Exists(_pathAndFileNameSet.ConfigurationFilePath);
        }

        public ModuloFiveConfiguration ReadConfiguration()
        {
            try
            {
                using FileStream fileStream = new(_pathAndFileNameSet.ConfigurationFilePath, FileMode.Open);
                // Native AOT Support
                var configuration = JsonSerializer.Deserialize<ModuloFiveConfiguration>(fileStream, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
                if (null == configuration)
                {
                    throw new ModuloFiveConfigurationException($"File {_pathAndFileNameSet.ConfigurationFilePath} contained the null object.");
                }
                return configuration;
            }

            catch (FileNotFoundException ex)
            {
                throw new ModuloFiveConfigurationException($"File {_pathAndFileNameSet.ConfigurationFilePath} does not exist.", ex);
            }
            catch (JsonException ex)
            {
                throw new ModuloFiveConfigurationException($"File {_pathAndFileNameSet.ConfigurationFilePath} contained invalid JSON.", ex);
            }
        }

        public void WriteConfiguration(ModuloFiveConfiguration configuration)
        {
            using (FileStream fileStream = new(_pathAndFileNameSet.ConfigurationWriteToFilePath, FileMode.OpenOrCreate))
            using (StreamWriter streamWriter = new(fileStream))
            {
                var json = JsonSerializer.Serialize(configuration, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
                streamWriter.Write(json);
            }

            File.Move(_pathAndFileNameSet.ConfigurationWriteToFilePath, _pathAndFileNameSet.ConfigurationFilePath, true);
        }
    }
}