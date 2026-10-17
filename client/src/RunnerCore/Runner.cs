using Microsoft.Extensions.Logging;
using ModuloFivePadClient.BotConnections;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.MacroPad.Serial;
using ModuloFivePadClient.Utilities.Extensions.ForEnumerables;
using Serilog;

namespace ModuloFivePadClient.RunnerCore
{
    public class Runner : IRunner
    {
        private readonly ILogger<Runner> _logger;
        private readonly IModuloFivePadConfigurationProvider _configurationProvider;
        private readonly ISwitchToggleModeWrapper _switchWrapper;
        private readonly IList<IBotConnectionBuilder> _connectionBuilders;
        private readonly List<RunnerLinkage> _runnerLinkages;
        private readonly object _lockObject;
        private readonly List<IBotConnection> _bots;

        public Runner(ILogger<Runner> logger, IModuloFivePadConfigurationProvider configurationProvider, ISwitchToggleModeWrapper switchWrapper, IEnumerable<IBotConnectionBuilder> connectionBuilders)
        {
            this._logger = logger;
            this._configurationProvider = configurationProvider;
            this._switchWrapper = switchWrapper;
            this._connectionBuilders = connectionBuilders.ToList();
            this._runnerLinkages = [];
            this._lockObject = new();
            this._bots = [];

            this._switchWrapper.SwitchStateChanged += SwitchStateChanged;
            foreach (var cb in this._connectionBuilders)
            {
                cb.BotConnected += BotConnected;
            }

            LoadConfiguration(this._configurationProvider.CloneCurrentConfigurationCopy());
            this._configurationProvider.ConfigurationChanged += LoadConfiguration;
        }

        private void BotConnected(IBotConnectionBuilder sender, IBotConnection botConnection)
        {
            this._bots.Add(botConnection);
        }

        private void SwitchStateChanged(IMacroPadConnection macroPad, LogicalSwitchStateChangedArgs args)
        {
            _logger.LogDebug("In {class}.{func}.", nameof(Runner), nameof(SwitchStateChanged));
            if (!args.LogicalSwitchStateChanged)
            {
                _logger.LogDebug("In {LogicalSwitchStateChanged} did not change.", nameof(args.LogicalSwitchStateChanged));
                return;
            }
            lock (this._lockObject)
            {
                foreach (var linkage in _runnerLinkages)
                {
                    bool doRun = false;
                    doRun |= linkage.TriggerState == SwitchState.Changed;
                    doRun |= linkage.TriggerState == SwitchState.On && args.LogicalSwitchState == SwitchState.On;
                    doRun |= linkage.TriggerState == SwitchState.Off && args.LogicalSwitchState == SwitchState.Off;

                    if (doRun)
                    {
                        var bots = _bots.Where(b => b.BotName == linkage.BotName);
                        var bot = bots.CheckAndLogTwoIsTooMany(nameof(bots), _logger);

                        if (bot == null)
                        {
                            _logger.LogWarning("In {class}.{func}, could not connect to bot {botName}.", nameof(Runner), nameof(SwitchStateChanged), linkage.BotName);
                            break;
                        }

                        if (
                            (linkage.MacroPadSerialId == macroPad.SerialId)
                         && (linkage.SwitchNumber == args.OriginalMessage.SwitchNumber)
                        )
                        {
                            List<BotArgument> arguments = [];

                            foreach (var linkageArg in linkage.Arguments)
                            {
                                if (linkageArg.Value != null)
                                {
                                    arguments.Add(new BotArgument(linkageArg.Name, linkageArg.Value));
                                }
                            }

                            bot.InvokeAction(linkage.BotActionId, arguments);
                            LinkagePerformed?.Invoke(this, new LinkagePerformedArgs(linkage));
                        }
                    }
                }
            }
        }

        public event IRunner.LinkagePerformedHandler? LinkagePerformed;

        public void LoadConfiguration(ModuloFiveConfiguration cfg)
        {
            lock (this._lockObject)
            {
                _runnerLinkages.Clear();
                foreach (var cfgLinkage in cfg.Linkages)
                {
                    if (null != cfgLinkage.TriggerState)
                    {
                        var newLinkage = new RunnerLinkage(cfgLinkage);
                        _runnerLinkages.Add(newLinkage);
                        if (newLinkage.Invalid)
                        {
                            break;
                        }

                        foreach (var argument in cfgLinkage.Arguments ?? [])
                        {
                            var newArgument = new RunnerLinkageArgument(argument);
                            if (!newArgument.Invalid)
                            {
                                // Invoke the action with as many arguments as we can.
                                newLinkage.Arguments.Add(newArgument);
                            }
                        }
                    }
                }
            }
        }
    }
}