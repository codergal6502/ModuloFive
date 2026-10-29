using Microsoft.Extensions.DependencyInjection;
using ModuloFivePadClient.BotConnections;
using ModuloFivePadClient.BotConnections.Http;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.MacroPad.Serial;
using ModuloFivePadClient.RunnerCore;
using ModuloFivePadClient.Utilities;
using Serilog;
using Serilog.Events;
using static ModuloFivePadClient.Configuration.ModuloFiveConfigurationFileSystemService;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        string[] arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();

        var fileLogEventLevel = Enum.GetValues<LogEventLevel>().OfType<LogEventLevel?>().FirstOrDefault(level => arguments.Any(arg => arg == $"--file-log-level={level}")) ?? LogEventLevel.Error;
        var consoleLogEventLevel = Enum.GetValues<LogEventLevel>().OfType<LogEventLevel?>().FirstOrDefault(level => arguments.Any(arg => arg == $"--console-log-level={level}")) ?? LogEventLevel.Error;
        var minimumLogEventLevel = Enum.GetValues<LogEventLevel>().OfType<LogEventLevel?>().FirstOrDefault(level => arguments.Any(arg => arg == $"--minimum-log-level={level}")) ?? LogEventLevel.Information;

        string logPath = Path.Combine(EnvironmentUtilities.EnsureConfigurationDirectory(), EnvironmentUtilities.LogFilePath);

        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLogEventLevel)
                .WriteTo.File(
                    path: logPath
                  , restrictedToMinimumLevel: fileLogEventLevel
                  , rollingInterval: RollingInterval.Day
                  , retainedFileCountLimit: 20
                  , rollOnFileSizeLimit: true
                  , fileSizeLimitBytes: 50 * 1024 * 1024
                )
                .WriteTo.Console(restrictedToMinimumLevel: consoleLogEventLevel)
                .CreateLogger();

        collection
            .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
            .AddSingleton<ISerialPortNameProvider, SerialPortNameProvider>()
            .AddSingleton<IMessageProcessorFactory, MessageProcessorFactory>()
            .AddSingleton<IMessageSerializerFactory, MessageSerializerFactory>()
            .AddSingleton<IMacroPadConnectionBuilder, SerialMacroPadConnectionManager>()
            .AddSingleton<IBotConnectionBuilder, HttpBotConnectionBuilder>()
            .AddSingleton<IHttpBotConnectionConfigurationProvider, TemporaryConfigProvider>()
            .AddSingleton<IModuloFivePadConfigurationProvider, ModuleFivePadApplicationDataJsonConfigurationProvider>()
            .AddSingleton<IRunner, Runner>()
            .AddSingleton<ISwitchToggleModeWrapper, SwitchToggleModeWrapper>()
            .AddSingleton<IModuloFiveConfigurationFileSystemService, ModuloFiveConfigurationFileSystemService>()
            .AddSingleton(PathAndFileNameSet.Default)
            .AddSingleton<Program>();
    }
}
