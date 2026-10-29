using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.DesktopClient.ViewModels;
using ModuloFivePadClient.Utilities;

namespace ModuloFivePadClient.DesktopClient.Models
{
#warning figure out where this should go
    public static class ObservationExtentions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> additions)
        {
            foreach (var a in additions)
            {
                collection.Add(a);
            }
        }

        public static bool AddUnique<T>(this ObservableCollection<T> collection, T addition)
        {
            if (collection.Any(t => EqualityComparer<T>.Default.Equals(t, addition)))
            {
                return false;
            }

            collection.Add(addition);
            return true;
        }

        public static void RemoveAll<T>(this ObservableCollection<T> collection)
        {
            foreach (var i in collection.Reverse())
            {
                collection.Remove(i);
            }
        }

        public static void RemoveRange<T>(this ObservableCollection<T> collection, IEnumerable<T> removals)
        {
            foreach (var r in removals)
            {
                collection.Remove(r);
            }
        }

        public static void ReplaceAll<T>(this ObservableCollection<T> collection, IEnumerable<T> additions)
        {
            collection.RemoveAll();
            collection.AddRange(additions);
        }
    }

    public static class ControlUtilities
    {
        public static void AutoSizeComboBox(object? obj)
        {
            if (obj is ComboBox cb)
            {
                AutoSizeComboBox(cb);
            }
        }

        #warning refactor to use attachedproperty, which would be more elegant
        public static void AutoSizeComboBox(ComboBox cb)
        {
            AutoSizeComboBox(cb, src => src.OfType<string>());
        }

        public static void AutoSizeComboBox<T>(object? obj, Func<T, String> stringMaker)
        {
            AutoSizeComboBox<T>(obj, l => l.Select(m => stringMaker(m)));
        }

        public static void AutoSizeComboBox<T>(object? obj, Func<IEnumerable<T>, IEnumerable<String>> stringMaker)
        {
            if (obj is ComboBox cb)
            {
                AutoSizeComboBox(cb, l => stringMaker(l.OfType<T>()));
            }
        }

        public static void AutoSizeComboBox(ComboBox cb, Func<IList, IEnumerable<String>> stringMaker)
        {
            cb.DataContextChanged += (_, _) =>
            {
                AutoSizeComboBox(cb);
            };
            var itemStrings = stringMaker(cb.Items.Source).ToList();
            double maxWidth = 0;

            // See https://stackoverflow.com/a/79916488/1102726.
            foreach (var itemString in itemStrings)
            {
                var ft = new FormattedText(
                    textToFormat: itemString,
                    culture: CultureInfo.CurrentUICulture,
                    flowDirection: cb.FlowDirection,
                    typeface: new Typeface(
                        fontFamily: cb.FontFamily,
                        style: cb.FontStyle,
                        weight: cb.FontWeight,
                        stretch: cb.FontStretch
                    ),
                    emSize: cb.FontSize, // a reasonable, uncited suggestion by Duck.AI
                    foreground: cb.Foreground
                );
                if (ft.WidthIncludingTrailingWhitespace > maxWidth)
                {
                    maxWidth = ft.WidthIncludingTrailingWhitespace;
                }
            }

            // the same reasonable, uncitewd suggestion also included accounting for the
            // padding and the arrow, though not exactly like this.
            maxWidth += cb.Padding.Left + cb.Padding.Right + 4 * cb.FontSize;

            cb.Width = maxWidth;
        }
    }

    public static class ViewModelUtilities
    {
        public static ModuloFiveConfiguration.SwitchMode? ConvertEnumCfg(ViewModels.SwitchMode? vmSwitchMode)
        {
            switch (vmSwitchMode)
            {
                case ViewModels.SwitchMode.Momentary:
                    return ModuloFiveConfiguration.SwitchMode.Momentary;
                case ViewModels.SwitchMode.SimulatedToggle:
                    return ModuloFiveConfiguration.SwitchMode.SimulatedToggle;
                case ViewModels.SwitchMode.PhysicalToggle:
                    return ModuloFiveConfiguration.SwitchMode.PhysicalToggle;
                default:
                    return null;
            }
        }

        public static ViewModels.SwitchMode ConvertEnumCfg(ModuloFiveConfiguration.SwitchMode? vmSwitchMode)
        {
            switch (vmSwitchMode)
            {
                case ModuloFiveConfiguration.SwitchMode.Momentary:
                default:
                    return ViewModels.SwitchMode.Momentary;
                case ModuloFiveConfiguration.SwitchMode.SimulatedToggle:
                    return ViewModels.SwitchMode.SimulatedToggle;
                case ModuloFiveConfiguration.SwitchMode.PhysicalToggle:
                    return ViewModels.SwitchMode.PhysicalToggle;
            }
        }

        public static ModuloFiveConfiguration.SwitchState? ConvertEnumCfg(ViewModels.SwitchState? vmSwitchState)
        {
            switch (vmSwitchState)
            {
                case ViewModels.SwitchState.On:
                    return ModuloFiveConfiguration.SwitchState.On;
                case ViewModels.SwitchState.Off:
                    return ModuloFiveConfiguration.SwitchState.Off;
                case ViewModels.SwitchState.Changed:
                    return ModuloFiveConfiguration.SwitchState.Changed;
                default:
                    return null;
            }
        }

        public static ViewModels.SwitchState ConvertEnumCfg(ModuloFiveConfiguration.SwitchState? vmSwitchState)
        {
            switch (vmSwitchState)
            {
                case ModuloFiveConfiguration.SwitchState.On:
                    return ViewModels.SwitchState.On;
                case ModuloFiveConfiguration.SwitchState.Off:
                    return ViewModels.SwitchState.Off;
                case ModuloFiveConfiguration.SwitchState.Changed:
                    return ViewModels.SwitchState.Changed;
                default:
                    return ViewModels.SwitchState.Unknown;
            }
        }

        public static void ClearAndPopulateFromConfiguration(this MainWindowViewModel vm, ModuloFiveConfiguration cfg)
        {
            vm.BotActions.ClearAndPopulateFromConfiguration(cfg);
            vm.MacroPadList.ClearAndPopulateFromConfiguration(cfg);
            vm.LinkageList.ClearAndPopulateFromConfiguration(cfg);
        }

        public static void ClearAndPopulateFromConfiguration(this BotActionsViewModel vm, ModuloFiveConfiguration cfg)
        {
            vm.Bots.RemoveAll();

            foreach (var cfgBot in cfg.Bots)
            {
                var vmBot = new BotViewModel
                {
                    IsEnabled = cfgBot.IsEnabled,
                    Name = cfgBot.Name,
                    #warning nickname not implemented yet
                    Nickname = cfgBot.Nickname ?? String.Empty,
                };
                vm.Bots.Add(vmBot);

                if (null != cfgBot.BotActions)
                {
                    foreach (var cfgAction in cfgBot.BotActions)
                    {
                        var actionVm = new BotActionViewModel
                        {
                            BotActionId = cfgAction.BotActionId,
                            Name = cfgAction.Name,
                        };

                        if (null != cfgAction.Parameters)
                        {
                            foreach (var param in cfgAction.Parameters)
                            {
                                actionVm.Parameters.Add(new BotActionParameterViewModel
                                {
                                    Name = param.Name,
                                });
                            }
                        }

                        vmBot.BotActions.Add(actionVm);
                    }
                }
            }

            vm.SelectedBot = vm.Bots.FirstOrDefault();
        }

        public static void ClearAndPopulateFromConfiguration(this MacroPadListViewModel vm, ModuloFiveConfiguration cfg)
        {
            vm.KnownMacroPads.RemoveAll();
            vm.DiscoveredMacroPads.RemoveAll();
            vm.CanAssignNewMacroPad = false;
            vm.IsAnyModalOpen = false;
            vm.IsNameMacroPadModalOpen = false;
            vm.IsNameMacroPadModalOpen = false;

            foreach (var cfgMp in cfg.MacroPads)
            {
                var vmMp = new KnownMacroPadViewModel
                {
                    IsConfigured = true,
                    IsOnline = false,
                    MacroPadVersion = cfgMp.Version,
                    Nickname = cfgMp.Nickname ?? string.Empty,
                    ProductId = cfgMp.ProductId,
                    SerialId = cfgMp.SerialId,
                };

                vmMp.Switches.AddRange(
                    cfgMp.Switches.Select(cfgSw => new SwitchViewModel
                    {
                        SwitchMode = ConvertEnumCfg(cfgSw.SwitchMode),
                        Nickname = cfgSw.Nickname ?? string.Empty,
                        LogicalState = ViewModels.SwitchState.Unknown,
                        SwitchNumberOneBased = cfgSw.SwitchNumber + 1
                    }
                    )
                );

                vm.KnownMacroPads.Add(vmMp);
            }
        }

        public static void ClearAndPopulateFromConfiguration(this LinkageListViewModel vm, ModuloFiveConfiguration cfg)
        {
            foreach (var cfgBot in cfg.Bots.Where(b => b.IsEnabled))
            {
                var newBotChoiceVm = new LinkageBotChoiceViewModel
                {
                    Name = cfgBot.Name ?? String.Empty,
                    Nickname = cfgBot.Nickname ?? String.Empty,
                };
                vm.ConfigureLinkage.BotChoices.Add(newBotChoiceVm);

                foreach (var cfgAction in cfgBot.BotActions ?? [])
                {
                    var newActionChoiceVm = new LinkageActionChoiceViewModel
                    {
                        Name = cfgAction.Name ?? String.Empty,
                        BotActionId = cfgAction.BotActionId ?? String.Empty,
                    };
                    newBotChoiceVm.ActionChoices.Add(newActionChoiceVm);

                    #warning why are some of the lists nullable?
                    foreach (var cfgParameter in cfgAction.Parameters ?? [])
                    {
                        var newParameterVm = new LinkageParameterViewModel
                        {
                            Name = cfgParameter.Name ?? String.Empty
                        };
                        newActionChoiceVm.Parameters.Add(newParameterVm);
                    }
                }
            }

            foreach (var cfgMacroPad in cfg.MacroPads)
            {
                var newMacroPadChoiceVm = new LinkageMacroPadChoice
                {
                    SerialId = cfgMacroPad.SerialId,
                    Nickname = cfgMacroPad.Nickname ?? String.Empty,
                };
                vm.ConfigureLinkage.MacroPadChoices.Add(newMacroPadChoiceVm);

                foreach (var cfgSwitch in cfgMacroPad.Switches)
                {
                    var newMacroSwitchChoiceVm = new LinkageSwitchChoice
                    {
                        SwitchNumberOneBased = cfgSwitch.SwitchNumber + 1,
                        Nickname = cfgSwitch.Nickname ?? String.Empty,
                    };

                    newMacroPadChoiceVm.SwitchChoices.Add(newMacroSwitchChoiceVm);
                }
            }

            foreach (var cfgLinkage in cfg.Linkages)
            {
                var linkageVm = new LinkageViewModel
                {
                    MacroPadSerialId = cfgLinkage.MacroPadSerialId,
                    MacroPadName = cfg.MacroPads.FirstOrDefault(mp => mp.SerialId == cfgLinkage.MacroPadSerialId)?.Nickname ?? String.Empty,
                    SwitchNumberOneBased = cfgLinkage.SwitchNumber + 1,
                    SwitchName = cfg.MacroPads.FirstOrDefault(mp => mp.SerialId == cfgLinkage.MacroPadSerialId)?.Switches?.FirstOrDefault(s => s.SwitchNumber == cfgLinkage.SwitchNumber)?.Nickname ?? String.Empty,
                    TriggerState = ConvertEnumCfg(cfgLinkage.TriggerState),
                    BotActionId = cfgLinkage.BotActionId ?? String.Empty,
                    BotName = cfgLinkage.BotName,
                    BotActionName = cfgLinkage.BotActionName,
                    ActionDeleted = false,
                };
                vm.Linkages.Add(linkageVm);

                foreach (var cfgArgument in cfgLinkage.Arguments ?? [])
                {
                    linkageVm.Arguments.Add(new BotActionArgumentViewModel
                    {
                        Name = cfgArgument.Name,
                        Value = cfgArgument.Value,
                        ParameterDeleted = false
                    });
                }
            }
        }
    }
}
