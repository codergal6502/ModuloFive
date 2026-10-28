using System.Text.Json;
using Microsoft.Extensions.Logging;
using ModuloFivePadClient.Configuration.Json;
using ModuloFivePadClient.Utilities;

namespace ModuloFivePadClient.Configuration
{
    using This = ModuleFivePadApplicationDataJsonConfigurationProvider;

    public class ModuleFivePadApplicationDataJsonConfigurationProvider : IModuloFivePadConfigurationProvider
    {
        private const string FILE_NAME = "settings.json";
        private const string FILE_NAME_WRITE_TO = "settings.json.temp";
        private readonly ILogger<This> _logger;
        private readonly IModuloFiveConfigurationFileSystemService _configurationFileSystemService;

        private static string ConfigurationPath { get => Path.Combine(EnvironmentUtilities.EnsureConfigurationDirectory(), FILE_NAME); }
        private static string ConfigurationWriteToPath { get => Path.Combine(EnvironmentUtilities.EnsureConfigurationDirectory(), FILE_NAME_WRITE_TO); }

        private ModuloFiveConfiguration _configuration;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true, };

        private static ModuloFiveConfiguration CloneConfiguration(ModuloFiveConfiguration original)
        {
            // Native AOT Support
            var json = JsonSerializer.Serialize(original, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
            var obj = JsonSerializer.Deserialize<ModuloFiveConfiguration>(json, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
            return obj ?? new ModuloFiveConfiguration();
        }

        public ModuleFivePadApplicationDataJsonConfigurationProvider(ILogger<This> logger, IModuloFiveConfigurationFileSystemService configurationFileSystemService)
        {
            _logger = logger;
            _configurationFileSystemService = configurationFileSystemService;
            _configuration = EnsureConfiguration();
        }

        public void WriteConfiguration()
        {
            SerializeToFile(_configuration);
        }

        private ModuloFiveConfiguration? ReadConfiguration()
        {
            using FileStream fileStream = new FileStream(ConfigurationPath, FileMode.Open);

            // Native AOT Support
            var configuration = JsonSerializer.Deserialize<ModuloFiveConfiguration>(fileStream, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
            return configuration;
        }

        private ModuloFiveConfiguration EnsureConfiguration()
        {
            ModuloFiveConfiguration? configuration = null;
            if (_configurationFileSystemService.ConfigurationFileExists())
            {
                _logger.LogInformation("Configuration file existed; attempting to load.");
                configuration = _configurationFileSystemService.ReadConfiguration();
            }

            if (null == configuration)
            {
                _logger.LogInformation("Configuration was null; writing to {0}.", ConfigurationPath);
                configuration = new ModuloFiveConfiguration();
                UpdateConfiguration(configuration, ConfigurationPersistenceMode.Immediate);
            }

            return configuration;
        }

        public ModuloFiveConfiguration CloneCurrentConfigurationCopy()
        {
            return CloneConfiguration(_configuration);
        }

        public void UpdateConfiguration(ModuloFiveConfiguration configuration, ConfigurationPersistenceMode configurationPersistenceMode = ConfigurationPersistenceMode.Immediate)
        {
            if (null == configuration)
            {
                throw new ModuloFiveConfigurationException($"In {nameof(UpdateConfiguration)}, cannot update; {nameof(configuration)} is null.");
            }

            ThrowIfInvalid(configuration);

            if (configurationPersistenceMode == ConfigurationPersistenceMode.Immediate)
            {
                SerializeToFile(configuration);
            }

            _configuration = CloneConfiguration(configuration);

            ConfigurationChanged?.Invoke(CloneConfiguration(configuration));
        }

        private void ThrowIfInvalid(ModuloFiveConfiguration configuration)
        {
            foreach (var bot in configuration.Bots)
            {
                if (string.IsNullOrWhiteSpace(bot.Name?.Trim()))
                {
                    throw new ModuloFiveConfigurationException($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Bot)}.{nameof(ModuloFiveConfiguration.Bot.Name)} is invalid.");
                }

                foreach (var action in bot.BotActions)
                {
                    if (string.IsNullOrWhiteSpace(action.Identifier))
                    {
                        throw new ModuloFiveConfigurationException($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.BotAction)}.{nameof(ModuloFiveConfiguration.BotAction.Identifier)} is invalid.");
                    }

                    if (string.IsNullOrWhiteSpace(action.Name))
                    {
                        throw new ModuloFiveConfigurationException($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.BotAction)}.{nameof(ModuloFiveConfiguration.BotAction.Name)} is invalid.");
                    }
                }
            }

            foreach (var macroPad in configuration.MacroPads)
            {
                // if (null == macroPad.SerialId)
                // {
                //     throw new ModuloFiveConfigurationException($"Configuration with null {nameof(ModuloFiveConfiguration.MacroPad)}.{nameof(ModuloFiveConfiguration.MacroPad.SerialId)} is invalid.");
                // }

                if (null == macroPad.Version)
                {
                    throw new ModuloFiveConfigurationException($"Configuration with null {nameof(ModuloFiveConfiguration.MacroPad)}.{nameof(ModuloFiveConfiguration.MacroPad.Version)} is invalid.");
                }

                // if (null == macroPad.ProductId)
                // {
                //     throw new ModuloFiveConfigurationException($"Configuration with null {nameof(ModuloFiveConfiguration.MacroPad)}.{nameof(ModuloFiveConfiguration.MacroPad.ProductId)} is invalid.");
                // }
            }

            foreach (var linkage in configuration.Linkages)
            {
                if (string.IsNullOrWhiteSpace(linkage.BotActionId))
                {
                    throw new ModuloFiveConfigurationException($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotActionId)} is invalid.");
                }

                if (string.IsNullOrWhiteSpace(linkage.BotActionName))
                {
                    throw new ModuloFiveConfigurationException($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotActionName)} is invalid.");
                }

                if (string.IsNullOrWhiteSpace(linkage.BotName))
                {
                    throw new ModuloFiveConfigurationException($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotName)} is invalid.");
                }

#warning remove these commented-out lines
                // if (null == linkage.MacroPadSerialId)
                // {
                //     throw new ModuloFiveConfigurationException($"Configuration with null {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.MacroPadSerialId)} is invalid.");
                // }

                // if (null == linkage.SwitchNumber)
                // {
                //     throw new ModuloFiveConfigurationException($"Configuration with null {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.SwitchNumber)} is invalid.");
                // }

                // if (null == linkage.TriggerState)
                // {
                //     throw new ModuloFiveConfigurationException($"Configuration with null {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.TriggerState)} is invalid.");
                // }

                foreach (var argument in linkage.Arguments)
                {
                    if (string.IsNullOrWhiteSpace(argument.Name))
                    {
                        throw new ModuloFiveConfigurationException($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.LinkageArgument)}.{nameof(ModuloFiveConfiguration.LinkageArgument.Name)} is invalid.");
                    }
                }
            }
        }

        private void SerializeToFile(ModuloFiveConfiguration uncorrectedConfiguration)
        {
            if (null == uncorrectedConfiguration)
            {
                throw new ModuloFiveConfigurationException($"In {nameof(SerializeToFile)}, cannot serialize; {nameof(uncorrectedConfiguration)} is null.");
            }

            var correctedConfiguration = CloneConfiguration(uncorrectedConfiguration);

            foreach (var bot in correctedConfiguration.Bots)
            {
                if (string.IsNullOrWhiteSpace(bot.Nickname))
                {
                    bot.Nickname = null;
                }
            }

            foreach (var argument in correctedConfiguration.Linkages.SelectMany(l => l.Arguments))
            {
                if (string.IsNullOrWhiteSpace(argument.Value))
                {
                    argument.Value = null;
                }
            }

            _configurationFileSystemService.WriteConfiguration(correctedConfiguration);

            // using (FileStream fileStream = new(ConfigurationWriteToPath, FileMode.OpenOrCreate))
            // using (StreamWriter streamWriter = new(fileStream))
            // {
            //     var json = JsonSerializer.Serialize(configuration, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
            //     streamWriter.Write(json);
            // }

            // File.Move(ConfigurationWriteToPath, ConfigurationPath, true);
        }

        // private ConcurrentQueue<Action<ModuloFiveConfiguration>> _configurationChangeQueue = new();
        // private Task? _configurationQueueReaderTask = null;
        // private object _configurationQueueLockObject = new();

        // public void EnqueueConfigurationChange(Action<ModuloFiveConfiguration> change)
        // {
        //     _configurationChangeQueue.Enqueue(change);
        //     lock (_configurationQueueLockObject)
        //     {
        //         if (null == _configurationQueueReaderTask)
        //         {
        //             _configurationQueueReaderTask = Task.Run(ConfigurationChangeQueueProcessor);

        //         }
        //     }
        // }
        // private readonly TimeSpan _configurationSaveRateLimit = new TimeSpan(0, 0, 15);
        // private DateTime _lastConfigurationSaveRequestTime = DateTime.MinValue;



        // private async Task<bool> RateLimitedSaveConfiguration()
        // {
        //     // unlike the button debounce in the arduino, where desired events are all the same (e.g., on)
        //     // and we can just use the first event as the desired event, we need the latest attempt to save
        //     // for saving.

        //     // Let's say we have a 15s rate limit.
        //     // 0s  4s  8s         19s 23s                   45s
        //     //
        //     // A   A   A          A   A                     ^       
        //     // |   |   |          |   |                     |
        //     // |   |   |          |   |                     + NOW - last request time = 22 >= 15, save immediately
        //     // |   |   |          |   |
        //     // |   |   |          |   + timer (B) elapsed, NOW - request time >= 15, save
        //     // |   |   |          |
        //     // |   |   |          + timer (A) elapsed, NOW - request time = 11 < 15, don't save
        //     // |   |   |
        //     // |   |   + NOW - last request time = 4 < 15, mark request time 4s, start a timer (B)
        //     // |   |
        //     // |   + NOW - last request time = 4 < 15, mark request time 4s, start a timer (A)
        //     // |
        //     // +-- NOW - forever ago >= 15, save immediately

        //     var firstNow = DateTime.Now;
        //     if (firstNow - _lastConfigurationSaveRequestTime > _configurationSaveRateLimit)
        //     {
        //         _lastConfigurationSaveRequestTime = firstNow;
        //         return true;
        //     }
        //     else
        //     {
        //         _lastConfigurationSaveRequestTime = firstNow;

        //         using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_configurationSaveDebounceCts.Token);

        //         try
        //         {
        //             await Task.Delay(_configurationSaveRateLimit, linkedCts.Token);
        //         }
        //         catch (OperationCanceledException ex) when (ex.CancellationToken == linkedCts.Token)
        //         {
        //             // If we were the last save request, indicate that we should save.
        //             return (_lastConfigurationSaveRequestTime == firstNow);
        //         }
        //         var secondNow = DateTime.Now;
        //         return secondNow - _lastConfigurationSaveRequestTime > _configurationSaveRateLimit;
        //     }
        // }

        // private void ConfigurationChangeQueueProcessor()
        // {
        //     while (this._configurationChangeQueue.TryDequeue(out Action<ModuloFiveConfiguration>? x))
        //     {
        //         if (null != x)
        //         {

        //         }
        //     }
        // }

        public ConfigurationChangeEvent? ConfigurationChanged { get; set; }
        public TimeSpan WriteBufferRateLimit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}