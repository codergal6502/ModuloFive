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

        public ModuleFivePadApplicationDataJsonConfigurationProvider(ILogger<This> logger)
        {
            this._logger = logger;
            _configuration = EnsureConfiguration();
        }

        public void WriteConfiguration()
        {
            SerializesToFile(_configuration);
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
            if (File.Exists(ConfigurationPath))
            {
                _logger.LogInformation("Configuration file {0} existed; attempting to load.", ConfigurationPath);
                configuration = ReadConfiguration();
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
            return CloneConfiguration(this._configuration);
        }

        public void UpdateConfiguration(ModuloFiveConfiguration configuration, ConfigurationPersistenceMode configurationPersistenceMode)
        {
            if (configurationPersistenceMode == ConfigurationPersistenceMode.Immediate)
            {
                SerializesToFile(configuration);
            }
            _configuration = CloneConfiguration(configuration);

            ConfigurationChanged?.Invoke(CloneConfiguration(configuration));
        }

        private void SerializesToFile(ModuloFiveConfiguration configuration)
        {
            if (null == configuration)
            {
                // todo: throw a better exception
                throw new NotImplementedException();
            }
            using (FileStream fileStream = new(ConfigurationWriteToPath, FileMode.OpenOrCreate))
            using (StreamWriter streamWriter = new(fileStream))
            {
                var json = JsonSerializer.Serialize(configuration, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
                streamWriter.Write(json);
            }

            File.Move(ConfigurationWriteToPath, ConfigurationPath, true);
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