using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ModuloFivePadClient.BotConnections.Http
{
    // Native AOT JSON Support 
    [JsonSerializable(typeof(BotAction[]))]
    [JsonSerializable(typeof(BotStatus))]
    [JsonSerializable(typeof(BotArgument[]))]
    [JsonSourceGenerationOptions(
        UseStringEnumConverter = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        IndentCharacter = ' ',
        IndentSize = 4,
        WriteIndented = true,
        AllowTrailingCommas = true
    )]
    internal partial class BotActionJsonContext : JsonSerializerContext
    {
    }

    // Native AOT JSON Support 
    public static class Preservative
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(BotAction))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(BotAction.Parameter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(BotArgument))]
        public static void CreateStuff()
        {
            _ = Activator.CreateInstance<BotAction>()!;
            _ = Activator.CreateInstance<BotAction.Parameter>()!;
            _ = Activator.CreateInstance<BotArgument>()!;
        }
    }

    public partial class HttpBotConnectionBuilder : IBotConnectionBuilder
    {
        static HttpBotConnectionBuilder()
        {
            Preservative.CreateStuff();
        }

        private readonly IHttpBotConnectionConfigurationProvider _httpActionConnectionConfigurationProvider;
        private readonly ILogger<HttpBotConnectionBuilder> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private ConcurrentBag<IBotConnection> _activeConnections;
        public IList<IBotConnection> ActiveBotConnections => _activeConnections.ToList();

        public event BotConnectedHandler? BotConnected;

        public HttpBotConnectionBuilder(IHttpBotConnectionConfigurationProvider httpActionConnectionConfigurationProvider, ILogger<HttpBotConnectionBuilder> logger, ILoggerFactory loggerFactory)
        {
            this._httpActionConnectionConfigurationProvider = httpActionConnectionConfigurationProvider;
            this._logger = logger;
            this._loggerFactory = loggerFactory;
            this._activeConnections = [];
        }

        public async Task<IList<IBotConnection>> BuildAllAsync(CancellationToken? cancellation)
        {
            _logger.LogDebug("In {class}.{method}, attempting to connect to {botCount} bots.", nameof(HttpBotConnectionBuilder), nameof(BuildAllAsync), _httpActionConnectionConfigurationProvider.Configurations.Count());
            List<IBotConnection> results = [];

            foreach (var cfg in _httpActionConnectionConfigurationProvider.Configurations)
            {
                var impl = new BotConnectionImpl(cfg, _loggerFactory.CreateLogger<BotConnectionImpl>());
                BotStatus botStatus = await impl.GetStatusAsync();

                _logger.LogDebug("In {class}.{method}, status of {bot} is {status}.", nameof(HttpBotConnectionBuilder), nameof(BuildAllAsync), cfg.Name, botStatus);
                if (botStatus == BotStatus.Available)
                {
                    this._activeConnections.Add(impl);
                    this.BotConnected?.Invoke(this, impl);
                    var _ = await impl.GetBotActionListsAsync();
                    results.Add(impl);
                }
            }

            return results;
        }
    }
}