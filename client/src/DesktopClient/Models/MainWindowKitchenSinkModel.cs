using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModuloFivePadClient.BotConnections;
using ModuloFivePadClient.DesktopClient.ViewModels;
using ModuloFivePadClient.MacroPad.Serial;
using InboundSwitchState = ModuloFivePadClient.MacroPad.Serial.SwitchState;
using OutboundSwitchState = ModuloFivePadClient.DesktopClient.ViewModels.SwitchState;
using System.Collections.ObjectModel;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.Utilities.Extensions.ForEnumerables;
using ModuloFivePadClient.RunnerCore;
using System.ComponentModel;
using System.Xml.Schema;

namespace ModuloFivePadClient.DesktopClient.Models
{
    internal static class Ext
    {
        public static OutboundSwitchState ToOutboundSwitchState(this MacroPad.Serial.SwitchState mpSwitchState)
        {
            switch (mpSwitchState)
            {
                case InboundSwitchState.Off:
                    {
                        return OutboundSwitchState.Off;
                    }
                case InboundSwitchState.On:
                    {
                        return OutboundSwitchState.On;
                    }
                case InboundSwitchState.Changed:
                    {
                        return OutboundSwitchState.Changed;
                    }
                case InboundSwitchState.Unknown:
                default:
                    {
                        return OutboundSwitchState.Unknown;
                    }
            }
        }
    }


    // See https://github.com/CommunityToolkit/MVVM-Samples/blob/master/samples/MvvmSample.Core/ViewModels/Widgets/ContactsListWidgetViewModel.cs#L77
    // See https://jkmills78.github.io/tutorials/mvvmexample/
#warning refactor this name
    public class MainWindowKitchenSinkModel : IDisposable
    {
        private readonly ILogger<MainWindowKitchenSinkModel> _logger;
        private readonly IModuloFivePadConfigurationProvider _moduloFivePadConfiguration;
        private readonly IMacroPadConnectionBuilder _macroPadConnectionBuilder;
        private readonly IEnumerable<IBotConnectionBuilder> _botConnectionBuilders;
        private readonly ISwitchToggleModeWrapper _switchWrapper;
        private readonly IRunner _runner;
        private readonly List<IBotConnection> _botConnections;
        private readonly List<IMacroPadConnection> _macroPadConnections;
        private object lockObjectForTemporarySaveThing = new();

        public MainWindowViewModel MainWindowViewModel { get; }

        public MainWindowKitchenSinkModel(ILogger<MainWindowKitchenSinkModel> logger, IRunner runner, ISwitchToggleModeWrapper switchWrapper, IModuloFivePadConfigurationProvider moduloFivePadConfiguration, IMacroPadConnectionBuilder macroPadConnectionBuilder, IEnumerable<IBotConnectionBuilder> botConnectionBuilders, MainWindowViewModel mainWindowViewModel)
        {
            this._logger = logger;
            this._moduloFivePadConfiguration = moduloFivePadConfiguration;
            this._macroPadConnectionBuilder = macroPadConnectionBuilder;
            this._botConnectionBuilders = botConnectionBuilders;
            this._switchWrapper = switchWrapper;
            this._runner = runner;
            this._botConnections = [];
            this._macroPadConnections = [];
            this.MainWindowViewModel = mainWindowViewModel;

            this._logger.LogDebug("In {modelFile} setting up c'tor available event handlers.", nameof(MainWindowKitchenSinkModel));

            _runner.LinkagePerformed += (_, args) =>
            {
                var macroPad = this.MainWindowViewModel.MacroPadList.KnownMacroPads.FirstOrDefault(mp => mp.SerialId == args.Linkage.MacroPadSerialId);
                var padSwitch = macroPad?.Switches?.FirstOrDefault(s => s.SwitchNumberOneBased == args.Linkage.SwitchNumber + 1);

                var bot = this.MainWindowViewModel.BotActions.Bots.FirstOrDefault(b => b.Name == args.Linkage.BotName);
                var botAction = bot?.BotActions?.FirstOrDefault(a => a.Identifier == args.Linkage.BotActionId);

                if (macroPad != null && padSwitch != null && bot != null && botAction != null)
                {
                    this.MainWindowViewModel.RunnerStatus =
                        String.Format(
                            "Switch {0} on macro pad {1} invoked action {2} on {3}.",
                            padSwitch.Nickname,
                            macroPad.Nickname,
                            botAction.Name,
                            bot.Name
                        );
                }
            };

            // this.MainWindowViewModel.MacroPadList.KnownMacroPads.CollectionChanged += ConfiguredMacroPadCollectionChanged;

            // See https://github.com/AvaloniaUI/Avalonia/discussions/13916 in case binding two commands to one button becomes relevant???

            this.MainWindowViewModel.MacroPadList.MacroPadsModified += MacroPadsModified;
            this.MainWindowViewModel.MacroPadList.MacroPadRescanRequested += RescanMacroPads;

            this.MainWindowViewModel.LinkageList.LinkagesModified += LinkagesModified;

            this.MainWindowViewModel.BotActions.ModuloFiveConfigurationChangeNeeded += BotsModified;

            this.MainWindowViewModel.OnReady += OnWindowViewModelReady;
        }

        #region Window Loading

        private void OnWindowViewModelReady()
        {
            // I found the initial Firebot connection was completing before
            // the configuration stored in the user directory was loaded, so
            // hopefully having this be an event on the view model removes
            // that race condition.
            MakeExternalConnections();
        }

        #endregion Window Loading

        #region External Connections

        private void MakeExternalConnections()
        {
            this._logger.LogDebug("In {modelFile}.{func}.", nameof(MainWindowKitchenSinkModel), nameof(MakeExternalConnections));

            this.MainWindowViewModel.MacroPadList.DiscoveredMacroPads.ReplaceAll([.. _macroPadConnectionBuilder.ActiveMacroPadConnections.Select(mp => new MacroPadViewModel { SerialId = mp.SerialId, MacroPadVersion = mp.MacroPadVersion, ProductId = mp.ProductId })]);

            _macroPadConnectionBuilder.MacroPadConnected += MacroPadConnected;

            foreach (var botConnectionBuilder in _botConnectionBuilders)
            {
                botConnectionBuilder.BotConnected += BotConnected;
                _ = botConnectionBuilder.BuildAllAsync();
            }

            _ = _macroPadConnectionBuilder.BuildAllAsync();
            this._logger.LogDebug("Done with {modelFile}.{func}.", nameof(MainWindowKitchenSinkModel), nameof(MakeExternalConnections));
        }

        private void RescanMacroPads()
        {
            _macroPadConnectionBuilder.BuildAllAsync();
        }

        private void BotConnected(IBotConnectionBuilder sender, IBotConnection botConnection)
        {
            botConnection.OnActionListRetrieved += BotActionListRetrievedHandler;
            _botConnections.Add(botConnection);

            AddBotConnectionToBotViewModels(this.MainWindowViewModel.BotActions.Bots, botConnection);

            AddBotConnectionToBotViewModels(this.MainWindowViewModel.LinkageList.ConfigureLinkage.BotChoices, botConnection);
        }

        private void AddBotConnectionToBotViewModels(ObservableCollection<BotViewModel> botViewModels, IBotConnection botConnection)
        {
            var existingBots = botViewModels.Where(vm => vm.Name == botConnection.BotName).ToList();
            var existingBot = existingBots.CheckAndLogTwoIsTooMany(nameof(existingBots), _logger);

            if (null == existingBot)
            {
                botViewModels.Add(new BotViewModel(botConnection));
            }
        }

        private void AddBotConnectionToBotViewModels(ObservableCollection<LinkageBotChoiceViewModel> botViewModels, IBotConnection botConnection)
        {
            var existingBots = botViewModels.Where(vm => vm.Name == botConnection.BotName).ToList();
            var bot = existingBots.CheckAndLogTwoIsTooMany(nameof(existingBots), _logger);

            if (null == bot)
            {
                bot = new LinkageBotChoiceViewModel { Name = botConnection.BotName };
                botViewModels.Add(bot);
            }
        }

        private void BotActionListRetrievedHandler(IBotConnection sender, IEnumerable<BotAction> retrievedBotActions)
        {
            PopulateBotChoices(sender, retrievedBotActions, this.MainWindowViewModel.BotActions);

            PopulateBotChoices(sender, retrievedBotActions, this.MainWindowViewModel.LinkageList);
            IdentifyLinkageErrors(sender, retrievedBotActions, this.MainWindowViewModel.LinkageList);
        }

        private void PopulateBotChoices(IBotConnection sender, IEnumerable<BotAction> retrievedBotActions, BotActionsViewModel botActionsViewModel)
        {
            var matchingBotVms = botActionsViewModel.Bots.Where(b => b.Name == sender.BotName).ToList();
            var matchingBotVm =
                matchingBotVms.CheckAndLogTwoIsTooMany(
                    nameof(matchingBotVms)
                  , _logger
                );

            if (null != matchingBotVm)
            {
                foreach (var retrievdAction in retrievedBotActions)
                {
                    var matchingBotActionVms = matchingBotVm.BotActions.Where(ba => ba.Name == retrievdAction.Name);
                    var matchingBotActionVm =
                        matchingBotActionVms.CheckAndLogTwoIsTooMany(nameof(matchingBotActionVms), _logger);

                    if (null == matchingBotActionVm)
                    {
                        matchingBotActionVm = new BotActionViewModel(retrievdAction);
                        matchingBotVm.BotActions.Add(matchingBotActionVm);
                    }
                    else
                    {
                        matchingBotActionVm.Name = retrievdAction.Name;
                        matchingBotActionVm.Identifier = retrievdAction.Identifier;
                    }

                    if (null != retrievdAction.Parameters)
                    {
                        foreach (var retreivedArgument in retrievdAction.Parameters)
                        {
                            var matchingArgumentsVm = matchingBotActionVm.Parameters.Where(arg => arg.Name == retreivedArgument.Name);
                            var matchingArgumentVm =
                                matchingArgumentsVm.CheckAndLogTwoIsTooMany(
                                    nameof(matchingArgumentsVm)
                                  , _logger
                                );

                            if (null == matchingArgumentVm)
                            {
                                matchingArgumentVm = new BotActionParameterViewModel(retreivedArgument);
                                matchingBotActionVm.Parameters.Add(matchingArgumentVm);
                            }
                            else
                            {
                                matchingArgumentVm.Type = retreivedArgument.Type;
                            }
                        }
                    }
                }
            }
        }

        private void PopulateBotChoices(IBotConnection sender, IEnumerable<BotAction> retrievedBotActions, LinkageListViewModel linkagesViewModel)
        {
            var matchingBotVms = linkagesViewModel.ConfigureLinkage.BotChoices.Where(b => b.Name == sender.BotName).ToList();
            var matchingBotVm =
                matchingBotVms.CheckAndLogTwoIsTooMany(
                    nameof(matchingBotVms)
                  , _logger
                );

            if (null != matchingBotVm)
            {
                foreach (var retrievdAction in retrievedBotActions)
                {
                    var matchingBotActionVms = matchingBotVm.ActionChoices.Where(ba => ba.Name == retrievdAction.Name);
                    var matchingBotActionVm =
                        matchingBotActionVms.CheckAndLogTwoIsTooMany(
                            nameof(matchingBotActionVms)
                          , _logger
                        );

                    if (null == matchingBotActionVm)
                    {
                        matchingBotActionVm = new LinkageActionChoiceViewModel(retrievdAction);
                        matchingBotVm.ActionChoices.Add(matchingBotActionVm);
                    }
                    else
                    {
                        matchingBotActionVm.Name = retrievdAction.Name ?? string.Empty;
                        matchingBotActionVm.Identifier = retrievdAction.Identifier ?? string.Empty;
                    }

                    if (null != retrievdAction.Parameters)
                    {
                        foreach (var retreivedArgument in retrievdAction.Parameters)
                        {
                            var matchingParametersVm = matchingBotActionVm.Parameters.Where(arg => arg.Name == retreivedArgument.Name);
                            var matchingParameterVm =
                                matchingParametersVm.CheckAndLogTwoIsTooMany(
                                    nameof(matchingParametersVm)
                                  , _logger
                                );

                            if (null == matchingParameterVm)
                            {
                                matchingParameterVm = new LinkageParameterViewModel(retreivedArgument);
                                matchingBotActionVm.Parameters.Add(matchingParameterVm);
                            }
                        }
                    }
                }
            }
        }

        private void IdentifyLinkageErrors(IBotConnection sender, IEnumerable<BotAction> retrievedBotActions, LinkageListViewModel linkagesViewModel)
        {
            linkagesViewModel.HasErrors = false;
            foreach (var linkageVm in linkagesViewModel.Linkages.Where(l => l.BotName == sender.BotName))
            {
                var matchingRetrievedBotActions = retrievedBotActions.Where(ba => ba.Name == linkageVm.BotActionName);
                var matchingRetrievedBotAction = matchingRetrievedBotActions.CheckAndLogTwoIsTooMany(nameof(matchingRetrievedBotActions), _logger);

                if (matchingRetrievedBotAction == null)
                {
                    linkageVm.ActionDeleted = true;
                    linkagesViewModel.HasErrors = true;
                }
                else
                {
                    linkageVm.HasErrors = false;
                    linkageVm.ActionDeleted = false;
                    var retrievedParameters = matchingRetrievedBotAction.Parameters ?? [];

                    foreach (var argumentVm in linkageVm.Arguments)
                    {
                        if (retrievedParameters.All(rp => rp.Name != argumentVm.Name))
                        {
                            argumentVm.ParameterDeleted = true;
                            linkageVm.HasErrors = true;
                            linkagesViewModel.HasErrors = true;
                        }
                    }

                    foreach (var parameter in retrievedParameters)
                    {
                        if (linkageVm.Arguments.All(vm => vm.Name != parameter.Name))
                        {
#warning consider making most of the veiew-model not null.
#warning remove parameter type: nobody uses it?
                            linkageVm.Arguments.Add(new BotActionArgumentViewModel
                            {
                                Name = parameter.Name,
                                ParameterDeleted = false,
                                Value = ""
                            });
                        }
                    }
                }
            }
        }

        private void MacroPadConnected(IMacroPadConnectionBuilder sender, IMacroPadConnection macroPadConnection)
        {
            // _switchWrapper.SwitchStateChanged += (_, args, switchState) =>
            // {

            // };
            // macroPadConnection.SwitchStateChanged += MacroPadSwitchChanged;
            _switchWrapper.SwitchStateChanged += MacroPadLogicalSwitchStateChanged;
            MacroPadConnected_MacroPadList(macroPadConnection);
        }

        private void MacroPadConnected_MacroPadList(IMacroPadConnection macroPadConnection)
        {
            IList<KnownMacroPadViewModel> knownMacroPadVms =
                this
                    .MainWindowViewModel
                    .MacroPadList
                    .KnownMacroPads
                    .Where(mp => mp.SerialId == macroPadConnection.SerialId)
                    .ToList();
            var knownMacroPadVm = knownMacroPadVms.CheckAndLogTwoIsTooMany(nameof(knownMacroPadVms), _logger);

            if (null != knownMacroPadVm)
            {
                if (knownMacroPadVm.ProductId != macroPadConnection.ProductId)
                {
                    _logger.LogWarning("For macro pad {serialId}, product ID changed from {old} to {new}; updating.", knownMacroPadVm.SerialId, knownMacroPadVm.ProductId, macroPadConnection.ProductId);
                    knownMacroPadVm.ProductId = macroPadConnection.ProductId;
                }

                if (knownMacroPadVm.MacroPadVersion != macroPadConnection.MacroPadVersion)
                {
                    _logger.LogWarning("For macro pad {serialId}, version changed from {old} to {new}; updating.", knownMacroPadVm.SerialId, knownMacroPadVm.MacroPadVersion, macroPadConnection.MacroPadVersion);
                    knownMacroPadVm.MacroPadVersion = macroPadConnection.MacroPadVersion;
                }

                knownMacroPadVm.IsConfigured = true;
                knownMacroPadVm.IsOnline = true;
            }
            else
            {
                IList<MacroPadViewModel> discoveredMacroPadVms =
                    this
                        .MainWindowViewModel
                        .MacroPadList
                        .DiscoveredMacroPads
                        .Where(mp => mp.SerialId == macroPadConnection.SerialId)
                        .ToList();
                var discoveredMacroPadVm = discoveredMacroPadVms.CheckAndLogTwoIsTooMany(nameof(discoveredMacroPadVms), _logger);

                if (null == discoveredMacroPadVm)
                {
                    discoveredMacroPadVm = new MacroPadViewModel
                    {
                        SerialId = macroPadConnection.SerialId,
                        MacroPadVersion = macroPadConnection.MacroPadVersion,
                        ProductId = macroPadConnection.ProductId
                    };
                    this.MainWindowViewModel.MacroPadList.DiscoveredMacroPads.Add(discoveredMacroPadVm);
                }
                else
                {
#warning there's not currently any code to support this scenario, e.g., removing the macro pad from _macroPadConnections, disposing, etc.
                    // This can happen if the pad becomes disconnected and reconnectred.
                    _logger.LogInformation("Macro pad {0} was discovered more than once.", macroPadConnection.SerialId);
                }

                this._macroPadConnections.Add(macroPadConnection);
            }
        }


        // private void MacroPadSwitchChanged(IMacroPadConnection macroPad, SwitchStateChangeMessage message)
        private void MacroPadLogicalSwitchStateChanged(IMacroPadConnection macroPad, LogicalSwitchStateChangedArgs args)
        {
            OutboundSwitchState physicalState = args.OriginalMessage.SwitchState.ToOutboundSwitchState();
            OutboundSwitchState logicalState = args.LogicalSwitchState.ToOutboundSwitchState();

            var macroPadSerialId = macroPad.SerialId;
            var switchNumberOneBased = args.OriginalMessage.SwitchNumber + 1;

            #region Macro Pad List

            MacroPadLogicalSwitchChanged_MacroPadList(macroPadSerialId, switchNumberOneBased, physicalState, logicalState);

            #endregion

            #region Macro Pad Configuration Modal Dialog

            if (this.MainWindowViewModel.MacroPadList.ConfigurationModalMacroPad.SerialId == macroPadSerialId)
            {
                var noExistingSwitch = true;
                foreach (var v in this.MainWindowViewModel.MacroPadList.ConfigurationModalMacroPad.Switches)
                {
                    if (v.SwitchNumberOneBased == switchNumberOneBased)
                    {
                        v.PhysicalState = physicalState;
                        v.LogicalState = logicalState;
                        noExistingSwitch = false;
                    }
                }

                if (noExistingSwitch)
                {
                    this.MainWindowViewModel.MacroPadList.ConfigurationModalMacroPad.Switches.Add(new SwitchViewModel
                    {
                        SwitchNumberOneBased = switchNumberOneBased,
                        PhysicalState = physicalState,
                        LogicalState = logicalState,
                    });
                }
            }

            #endregion Macro Pad Configuration Modal Dialog

            #region Linkages

            var linkage =
                this
                    .MainWindowViewModel
                    .LinkageList
                    .Linkages
                    .FirstOrDefault(l => l.MacroPadSerialId == macroPadSerialId && l.SwitchNumberOneBased == switchNumberOneBased);

            if (null != linkage)
            {
                // linkage.
            }

            #endregion
        }

        private void MacroPadLogicalSwitchChanged_MacroPadList(
            Guid macroPadSerialId,
            int switchNumberOneBased,
            OutboundSwitchState physicalState,
            OutboundSwitchState logicalState)
        {
            var macroPadViewModel = this.MainWindowViewModel.MacroPadList.KnownMacroPads.FirstOrDefault(mp => mp.SerialId == macroPadSerialId);
            var switchViewModel = macroPadViewModel?.Switches?.FirstOrDefault(sw => sw.SwitchNumberOneBased == switchNumberOneBased);

            if (switchViewModel != null)
            {
                switchViewModel.PhysicalState = physicalState;
                switchViewModel.LogicalState = logicalState;
            }
        }

        #endregion External Connctions

        #region Configuration Change Request Handlers

        private void BotsModified(BotActionsViewModel sender)
        {
            lock (lockObjectForTemporarySaveThing)
            {
                var cfg = _moduloFivePadConfiguration.CloneCurrentConfigurationCopy();
                cfg.Bots =
                    sender
                        .Bots
                        .Select(b => new ModuloFiveConfiguration.Bot
                        {
                            IsEnabled = b.IsEnabled,
                            Name = b.Name,
                            Nickname = b.Nickname,
                            BotActions = b.BotActions.Select(a => new ModuloFiveConfiguration.BotAction
                            {
                                Identifier = a.Identifier,
                                Name = a.Name,
                                Parameters = a.Parameters.Select(p => new ModuloFiveConfiguration.BotActionParameter
                                {
                                    Name = p.Name,
                                    Type = p.Type
                                }).ToArray()
                            }).ToArray()
                        })
                        .ToList();
                _moduloFivePadConfiguration.UpdateConfiguration(cfg);
            }
        }

        private void MacroPadsModified(MacroPadListViewModel sender)
        {
            foreach (var kmp in sender.KnownMacroPads)
            {
                var linkageMacroPadChoices = MainWindowViewModel.LinkageList.ConfigureLinkage.MacroPadChoices.Where(lmp => lmp.SerialId == kmp.SerialId);
                var lmp = linkageMacroPadChoices.CheckAndLogTwoIsTooMany(nameof(linkageMacroPadChoices), _logger);

                if (null == lmp)
                {
                    lmp = new() { SerialId = kmp.SerialId };
                    MainWindowViewModel.LinkageList.ConfigureLinkage.MacroPadChoices.Add(lmp);
                }

                lmp.Nickname = kmp.Nickname;
                foreach (var ks in kmp.Switches)
                {
                    var linkedSwitchChoices = lmp.SwitchChoices.Where(ls => ls.SwitchNumberOneBased == ks.SwitchNumberOneBased);
                    var ls = linkedSwitchChoices.CheckAndLogTwoIsTooMany(nameof(linkedSwitchChoices), _logger);

                    if (null == ls)
                    {
                        ls = new() { SwitchNumberOneBased = ks.SwitchNumberOneBased };
                        lmp.SwitchChoices.Add(ls);
                    }

                    ls.Nickname = ks.Nickname;
                }
            }

            lock (lockObjectForTemporarySaveThing)
            {
                var cfg = _moduloFivePadConfiguration.CloneCurrentConfigurationCopy();
                cfg.MacroPads =
                    [.. sender.KnownMacroPads.Select(mpvm => new ModuloFiveConfiguration.MacroPad {
                        Nickname = mpvm.Nickname,
                        ProductId = mpvm.ProductId,
                        SerialId = mpvm.SerialId,
                        Version = mpvm.MacroPadVersion,
                        Switches = [.. mpvm.Switches.Select(svm => new ModuloFiveConfiguration.Switch {
                            Nickname = svm.Nickname,
                            SwitchMode = ViewModelUtilities.ConvertEnumCfg(svm.SwitchMode),
                            SwitchNumber = svm.SwitchNumberOneBased - 1,
                        })]
                    })];
                _moduloFivePadConfiguration.UpdateConfiguration(cfg);
            }
        }

        private void LinkagesModified(LinkageListViewModel sender)
        {
            lock (lockObjectForTemporarySaveThing)
            {
                var cfg = _moduloFivePadConfiguration.CloneCurrentConfigurationCopy();
                cfg.Linkages =
                    [.. sender.Linkages.Select(l => new ModuloFiveConfiguration.Linkage {
                        MacroPadSerialId = l.MacroPadSerialId,
                        SwitchNumber = l.SwitchNumberOneBased - 1,
                        TriggerState = ViewModelUtilities.ConvertEnumCfg(l.TriggerState),
                        BotName = l.BotName,
                        BotActionId = l.BotActionId,
                        BotActionName = l.BotActionName,
                        Arguments = [..l.Arguments.Select(a => new ModuloFiveConfiguration.LinkageArgument{
                            Name = a.Name,
                            Value = a.Value
                        })]
                    })];
                _moduloFivePadConfiguration.UpdateConfiguration(cfg);
            }
        }

        #endregion Configuration Change Request Handlers

        public void Dispose()
        {
            _macroPadConnectionBuilder.MacroPadConnected -= MacroPadConnected;
            foreach (var botConnectionBuilder in _botConnectionBuilders)
            {
                botConnectionBuilder.BotConnected -= BotConnected;
            }

            foreach (var botConnection in this._botConnections)
            {
                botConnection.OnActionListRetrieved -= BotActionListRetrievedHandler;
            }

            foreach (var macroPadConnection in this._macroPadConnections)
            {
                _switchWrapper.SwitchStateChanged -= MacroPadLogicalSwitchStateChanged; ;
            }
        }
    }
    public class MacroPadSwitchChangedHandlerArgs
    {
        public Guid? MacroPadSerialId { get; internal set; }
        public OutboundSwitchState SwitchState { get; internal set; }
        public int SwitchNumberOneBased { get; internal set; }
    }
    public delegate void MacroPadSwitchChangedHandler(MainWindowKitchenSinkModel sender, MacroPadSwitchChangedHandlerArgs args);
}