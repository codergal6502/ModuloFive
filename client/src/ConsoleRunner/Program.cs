using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModuloFivePadClient.BotConnections;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.MacroPad.Serial;
using ModuloFivePadClient.RunnerCore;
using static ModuloFivePadClient.Configuration.ModuloFiveConfiguration;

public class Program
{
    private const string QUIT = "QUIT";
    private readonly IMacroPadConnectionBuilder _macroPadConnectionBuilder;
    private readonly IEnumerable<IBotConnectionBuilder> _botConnectionBuilders;
    private readonly IRunner _runner;
    private readonly IModuloFivePadConfigurationProvider _configurationProvider;
    private readonly ILogger _logger;
    private readonly ModuloFiveConfiguration _configuration;
    private List<IMacroPadConnection> _allMacroPadConnections;

    public async static Task Main(string[] args)
    {
        var collection = new ServiceCollection();
        collection.AddCommonServices();

        var services = collection.BuildServiceProvider();
        Program program = services.GetRequiredService<Program>();
        await program.Setup();
        await program.StallForever();
    }

    public Program(
        IMacroPadConnectionBuilder macroPadConnectionBuilder,
        IEnumerable<IBotConnectionBuilder> botConnectionBuilders,
        IRunner runner,
        IModuloFivePadConfigurationProvider configurationProvider,
        ILogger<Program> logger
    )
    {
        _macroPadConnectionBuilder = macroPadConnectionBuilder;
        _botConnectionBuilders = botConnectionBuilders;
        _runner = runner;
        _configurationProvider = configurationProvider;
        _logger = logger;
        _allMacroPadConnections = [];

        _configuration = _configurationProvider.CloneCurrentConfigurationCopy();

        AppDomain.CurrentDomain.ProcessExit += ProcessExit;
    }

    private void ProcessExit(object? sender, EventArgs e)
    {
        foreach (var padConn in _allMacroPadConnections)
        {
            try
            {
                padConn.Dispose();
            }
            catch
            {

            }
        }
    }

    async Task Setup()
    {
        _runner.LoadConfiguration(_configurationProvider.CloneCurrentConfigurationCopy());
        List<Bot> expectedBots = [.. _configuration.Bots.Where(b => b.IsEnabled == true)];
        List<MacroPad> expectedPads = [.. _configuration.MacroPads];

        var expectedBotCount = expectedBots.Count;
        var expectedPadCount = expectedPads.Count;

        List<IBotConnection> connectedBots = [];
        List<IMacroPadConnection> connectedPads = [];

        var botTask = Task.Run(async () =>
        {
            foreach (var botConnectionBuilder in _botConnectionBuilders)
            {
                botConnectionBuilder.BotConnected += BotConnected;
                connectedBots = [.. await botConnectionBuilder.BuildAllAsync()];
            }
        });

        _macroPadConnectionBuilder.MacroPadConnected += MacroPadConnected;
        var padTask = Task.Run(async () => connectedPads = [.. await _macroPadConnectionBuilder.BuildAllAsync()]);

        await Task.WhenAll(botTask, padTask);

        if (expectedBotCount != connectedBots.Count)
        {
            expectedBots.RemoveAll(b1 => connectedBots.Any(b2 => b1.Name == b2.BotName));
            Console.Out.WriteLine(
                "♦ ♢ ♦ Only connected to {0}/{1} bots; could not connect to {2}.",
                connectedBots.Count,
                expectedBotCount,
                String.Join(", ", expectedBots.Select(b => b.Name))
            );
        }

        if (expectedPadCount != connectedPads.Count)
        {
            expectedPads.RemoveAll(b1 => connectedPads.Any(b2 => b1.SerialId == b2.SerialId));
            Console.Out.WriteLine(
                "♦ ♢ ♦ Only connected to {0}/{1} macro pads; could not connect to {2}.",
                connectedPads.Count,
                expectedPadCount,
                String.Join(", ", expectedPads.Select(b => b.Nickname ?? b.SerialId.ToString()))
            );
        }

        _runner.LoadConfiguration(_configuration);
        _runner.LinkagePerformed += LinkagePerformed;
    }

    private void BotConnected(IBotConnectionBuilder sender, IBotConnection botConnection)
    {
        Console.Out.WriteLine($"♥ ♡ ♥ Connected to {botConnection.BotName} ♥ ♡ ♥");
    }

    private void MacroPadConnected(IMacroPadConnectionBuilder sender, IMacroPadConnection macroPadConnection)
    {
        var macroPadNickname = FirstNonEmptyString(GetMacroPadNickname(macroPadConnection.SerialId), macroPadConnection.SerialId.ToString());
        _allMacroPadConnections.Add(macroPadConnection);
        Console.Out.WriteLine($"♡ ♥ ♡ Connected to {macroPadNickname} ♡ ♥ ♡");
    }

    private MacroPad? GetMacroPad(Guid? serialId)
    {
        return _configuration.MacroPads.FirstOrDefault(mp => mp.SerialId == serialId);
    }

    private string? GetMacroPadNickname(Guid? serialId)
    {
        return FirstNonEmptyString(GetMacroPad(serialId)?.Nickname, serialId.ToString());
    }

    private void GetMacroPadAndSwitchNicknames(Guid? macroPadSerialId, int? switchNumber, out string? macroPadNickname, out string? switchNickname)
    {
        var macroPad = GetMacroPad(macroPadSerialId);
        macroPadNickname = FirstNonEmptyString(macroPad?.Nickname, macroPadSerialId?.ToString());

        var macroPadSwitch = GetSwitch(macroPad, switchNumber);
        switchNickname = macroPadSwitch?.Nickname;
    }

    private Switch? GetSwitch(MacroPad? macroPad, int? switchNumber)
    {
        return macroPad?.Switches?.FirstOrDefault(s => s.SwitchNumber == switchNumber);
    }

    private Bot? GetBot(string? botName)
    {
        return _configuration.Bots.FirstOrDefault(b => b.Name == botName);
    }

    private ModuloFiveConfiguration.BotAction? GetBotAction(Bot? bot, string? actionId)
    {
        return bot?.BotActions?.FirstOrDefault(a => a.Identifier == actionId);
    }

    private void GetBotAndActionNicknames(string? botName, string? actionId, out string? botNickname, out string? actionName)
    {
        var bot = GetBot(botName);
        botNickname = FirstNonEmptyString(bot?.Nickname, botName);

        var action = GetBotAction(bot, actionId);
        actionName = FirstNonEmptyString(action?.Name, actionId);
    }

    private void LinkagePerformed(IRunner sender, LinkagePerformedArgs args)
    {
        var linkage = args.Linkage;
        var macroPadSerialId = linkage.MacroPadSerialId;
        var switchNumber = linkage.SwitchNumber;

        var botName = linkage.BotName;
        var actionId = linkage.BotActionId;

        GetMacroPadAndSwitchNicknames(macroPadSerialId, switchNumber, out string? macroPadNickname, out string? switchNickname);
        GetBotAndActionNicknames(botName, actionId, out string? botNickname, out string? actionName);

        Console.Out.WriteLine(
            "{4}: Switch ♣{0}♣ on macro pad ♧{1}♧ invoked action ♣{2}♣ on ♧{3}♧",
            switchNickname,
            macroPadNickname,
            actionName,
            botNickname,
            DateTime.Now.ToString("HH:mm:ss.ffff")
        );
    }

    private async Task StallForever()
    {
        Console.Out.WriteLine("♢ ♦ ♢ To quit, type the word 'QUIT' and press enter, or just close the program ♢ ♦ ♢");
        while (true)
        {
            var quit1 = await Console.In.ReadLineAsync() ?? string.Empty;

            if (QUIT == quit1.ToUpper().Trim())
            {
                Console.Out.WriteLine("♦ ♢ ♦ If you're sure, type 'QUIT' and press enter again ♦ ♢ ♦");
            }

            var quit2 = await Console.In.ReadLineAsync() ?? string.Empty;

            if (QUIT == quit2.ToUpper().Trim())
            {
                return;
            }
            else
            {
                Console.Out.WriteLine("♢ ♦ ♢ To quit, type the word 'QUIT' and press enter, or just close the program ♢ ♦ ♢");
            }
        }
    }

    static string FirstNonEmptyString(params string?[] strings)
    {
        return strings.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?.Trim() ?? "(unknown)";
    }
}