using System;
using System.Collections.Generic;
using ModuloFivePadClient.DesktopClient.Models;
using ModuloFivePadClient.DesktopClient.ViewModels;
using ModuloFivePadClient.MacroPad.Serial;

namespace ModuloFivePadClient.DesktopClient.DesignArtifacts
{
    // What's with all the lambda expression nonsense here? Well, as a matter of defensive programming,
    // all of the view model ObservableCollection<T> properties are getter-only properties. That way,
    // there's no missing "changes that aren't changes" to the collections when they get assigned. The
    // only way to change the collections is to change the collections. That does mean that assigning
    // the properties new values is impossible. So any view model that has an ObservableCollection gets
    // built in its own lambda expression.
    public static class DesignData
    {
        public static MainWindowViewModel MainWindowViewModel
        {
            get => new()
            {
                SelectedActivity = ViewModels.MainWindowViewModel.MENU_ITEM_NAME_LINKAGES,
                MacroPadList = DesignData.MacroPadListViewModel,
                BotActions = DesignData.BotActionsViewModel,
                LinkageList = DesignData.LinkageListViewModel
            };
        }

        public static MacroPadListViewModel MacroPadListViewModel
        {
            get
            {
                MacroPadListViewModel result = ((Func<MacroPadListViewModel>)(() =>
                {
                    MacroPadListViewModel result = new();
                    result.DiscoveredMacroPads.AddRange([
                        new MacroPadViewModel {
                            ProductId = PadConstants.MACRO_PAD_PRODUCT_ID,
                            MacroPadVersion = new Version(1, 2, 3),
                            SerialId = new Guid("2d73cbde-23ba-4f1b-b904-fb45341dc185"),
                        },
                        new MacroPadViewModel {
                            ProductId = PadConstants.MACRO_PAD_PRODUCT_ID,
                            MacroPadVersion = new Version(1, 3, 5),
                            SerialId = new Guid("64a17b27-735b-4e1c-8880-12f8bb07133f"),
                        }
                    ]);
                    result.KnownMacroPads.AddRange(
                        [
                            ((Func<KnownMacroPadViewModel>)(() => {
                                var result = new KnownMacroPadViewModel{
                                    ProductId = PadConstants.MACRO_PAD_PRODUCT_ID,
                                    MacroPadVersion = new Version(1, 2, 3),
                                    SerialId = new Guid("1798e6ee-2eb5-4402-bb41-37dbd0f90f37"),
                                    Nickname = "Foot Pedals",
                                    IsConfigured = true,
                                    IsOnline = false
                                };

                                result.Switches.AddRange([
                                    new SwitchViewModel {
                                        Nickname = "Left Pedal",
                                        SwitchNumberOneBased = 1,
                                        SwitchMode = SwitchMode.Momentary,
                                        LogicalState = ViewModels.SwitchState.Unknown

                                    },
                                    new SwitchViewModel {
                                        Nickname = "Right Pedal",
                                        SwitchNumberOneBased = 2,
                                        SwitchMode = SwitchMode.SimulatedToggle,
                                        LogicalState = ViewModels.SwitchState.Off
                                    }
                                ]);

                                return result;
                            }))(),
                            ((Func<KnownMacroPadViewModel>)(() => {
                                var result = new KnownMacroPadViewModel{
                                    ProductId = PadConstants.MACRO_PAD_PRODUCT_ID,
                                    MacroPadVersion = new Version(1, 3, 5),
                                    SerialId = new Guid("2e21fe53-f799-4d8e-b612-30d34febf816"),
                                    Nickname = "Desk Switches",
                                    IsConfigured = true,
                                    IsOnline = true,
                                };

                                result.Switches.AddRange([
                                    new SwitchViewModel {
                                        Nickname = "Left Switch",
                                        SwitchNumberOneBased = 1,
                                        SwitchMode = SwitchMode.SimulatedToggle,
                                        LogicalState = ViewModels.SwitchState.Unknown

                                    },
                                    new SwitchViewModel {
                                        Nickname = "Center Switch",
                                        SwitchNumberOneBased = 2,
                                        SwitchMode = SwitchMode.SimulatedToggle,
                                        LogicalState = ViewModels.SwitchState.Unknown

                                    },
                                    new SwitchViewModel {
                                        Nickname = "Right Button",
                                        SwitchNumberOneBased = 3,
                                        SwitchMode = SwitchMode.Momentary,
                                        LogicalState = ViewModels.SwitchState.Off
                                    }
                                ]);

                                return result;
                            }))()
                        ]
                    );

                    return result;
                }))();

                return result;
            }
        }

        public static KnownMacroPadViewModel ConfigureExistingMacroPadViewModel
        {
            get => new KnownMacroPadViewModel()
            {
                IsConfigured = true,
                MacroPadVersion = new Version(1, 2, 3),
                IsOnline = true,
                Nickname = "Foo Bar Pad",
                ProductId = new Guid("209d7304-0b49-4992-ac14-a258b2d15b31"),
                SerialId = new Guid("eec72a94-bb0b-4e4d-8b90-f344bacc9e21"),
                Switches = {
                    new SwitchViewModel {
                        SwitchMode = SwitchMode.Momentary,
                        LogicalState = ViewModels.SwitchState.Unknown,
                        Nickname = "Foo",
                        SwitchNumberOneBased = 1
                    },
                    new SwitchViewModel {
                        SwitchMode = SwitchMode.SimulatedToggle,
                        LogicalState = ViewModels.SwitchState.Unknown,
                        Nickname = "Bar",
                        SwitchNumberOneBased = 2
                    }
                }
            };
        }

        public static BotActionsViewModel BotActionsViewModel
        {
            get
            {
                BotActionsViewModel result = new();
                result.Bots.AddRange([
                    ((Func<BotViewModel>)(() => {
                        var result = new BotViewModel{
                             Name = "Firebot"
                           , IsEnabled = true
                           , Nickname = "Fredregar"
                        };

                        result.BotActions.AddRange([
                            ((Func<BotActionViewModel>)(() => {
                                var result = new BotActionViewModel {
                                    Name = "Mark Timestamp"
                                  , Identifier = "d7b18e37-bed7-4c8b-94c0-2448192ef1b9"
                                };

                                result.Parameters.AddRange([ new BotActionParameterViewModel {
                                    Name = "Reason"
                                }]);

                                return result;
                            }))(),
                            ((Func<BotActionViewModel>)(() => {
                                var result = new BotActionViewModel {
                                    Name = "Disable Noisy Effects Queue"
                                  , Identifier = "bdac3c31-fbc3-4284-80ab-b1a51578fb36"
                                };

                                result.Parameters.AddRange([
                                    new BotActionParameterViewModel {
                                        Name = "Delay",
                                        Type = null
                                    },
                                    new BotActionParameterViewModel {
                                        Name = "Interrupt",
                                        Type = null
                                    }
                                ]);

                                return result;
                            }))(),
                            ((Func<BotActionViewModel>)(() => {
                                var result = new BotActionViewModel {
                                    Name = "Enable Noisy Effects Queue"
                                  , Identifier = "ad356fa8-9dca-4b35-9ae0-6b83bf375690"
                                };

                                return result;
                            }))(),
                        ]);

                        return result;
                    }))(),
                    ((Func<BotViewModel>)(() => {
                        var result = new BotViewModel{
                            Name = "MixItUp",
                            IsEnabled = false,
                            Nickname = "Meriadoc"
                        };

                        return result;
                    }))(),
                ]);
                result.SelectedBot = result.Bots[0];

                // if (string.Empty.Length == 0) throw new InvalidOperationException($"selectedBot = ({result.SelectedBot.Name}, isEnabled={result.SelectedBot.IsEnabled}, nickname={result.SelectedBot.Nickname}); isEnabled={result.IsEnabled}, nickname={result.Nickname}; result.selectedBot == result.Bots[0]: {result.SelectedBot == result.Bots[0]}");


                if (result.SelectedBotName != result.SelectedBot.Name)
                {
                    throw new InvalidOperationException($"result.SelectedBotName ({result.SelectedBotName}) != result.SelectedBot.Name ({result.SelectedBot.Name})");
                }

                if (result.IsEnabled != result.SelectedBot.IsEnabled)
                {
                    throw new InvalidOperationException($"result.IsEnabled ({result.IsEnabled}) != result.SelectedBot.IsEnabled ({result.SelectedBot.IsEnabled})");
                }

                return result;
            }
        }

        public static LinkageListViewModel LinkageListViewModel
        {
            get => new()
            {
                HasErrors = true,
                Linkages = {
                    new LinkageViewModel {
                        SwitchName = "Left",
                        SwitchNumberOneBased = 1,
                        MacroPadName = "Foot Pedals",
                        MacroPadSerialId = new Guid("1b4b942b-04fe-4a30-b4c5-a5ebd91017b3"),
                        BotActionName = "Pause Noisy Events",
                        BotActionId = "051bf154-8250-41de-9ebf-71371e981b01",
                        BotName = "Firebot",
                        TriggerState = ViewModels.SwitchState.On,
                        HasErrors = false,
                        Arguments = {
                            new() {
                                Name = "Duration",
                                Value = "60000",
                                ParameterDeleted = false,
                            }
                        }
                    },
                    new LinkageViewModel {
                        SwitchName = "Right",
                        SwitchNumberOneBased = 1,
                        MacroPadName = "Foot Pedals",
                        MacroPadSerialId = new Guid("f207d41d-28f7-4e9c-b2ba-a0eced3e3388"),
                        BotActionName = "Timestamp for Recap",
                        BotActionId = "894ed370-b501-41cd-b515-c9735d9be9e8",
                        BotName = "Firebot",
                        TriggerState = ViewModels.SwitchState.On,
                        HasErrors = false,
                    },
                    new LinkageViewModel {
                        SwitchName = "Has Errors",
                        SwitchNumberOneBased = 1,
                        MacroPadName = "Another Pad",
                        MacroPadSerialId = new Guid("f207d41d-28f7-4e9c-b2ba-a0eced3e3388"),
                        BotActionName = "Do Something",
                        BotActionId = "894ed370-b501-41cd-b515-c9735d9be9e8",
                        BotName = "Firebot",
                        TriggerState = ViewModels.SwitchState.On,
                        HasErrors = true,
                    },
                }
            };
        }

        public static ConfigureLinkageViewModel DesignTimeConfigureLinkageViewModel
        {
            get
            {
                var result =  new ConfigureLinkageViewModel();

                result.MacroPadChoices.AddRange([
                    ((Func<LinkageMacroPadChoice>)(() =>
                    {
                        var result = new LinkageMacroPadChoice{
                            SerialId = new Guid("1798e6ee-2eb5-4402-bb41-37dbd0f90f37"),
                            Nickname = "Foot Pedals",
                        };

                        result.SwitchChoices.AddRange([
                            new LinkageSwitchChoice {
                                Nickname = "Left Pedal",
                                SwitchNumberOneBased = 1,

                            },
                            new LinkageSwitchChoice {
                                Nickname = "Right Pedal",
                                SwitchNumberOneBased = 2,
                            }
                        ]);

                        return result;
                    }))(),
                    ((Func<LinkageMacroPadChoice>)(() =>
                    {
                        var result = new LinkageMacroPadChoice{
                            SerialId = new Guid("2e21fe53-f799-4d8e-b612-30d34febf816"),
                            Nickname = "Desk Switches",
                        };

                        result.SwitchChoices.AddRange([
                            new LinkageSwitchChoice {
                                Nickname = "Left",
                                SwitchNumberOneBased = 1,
                            },
                            new LinkageSwitchChoice {
                                Nickname = "Center",
                                SwitchNumberOneBased = 2,
                            }
                        ]);

                        return result;
                    }))(),
                ]);

                result.BotChoices.AddRange([
                    ((Func<LinkageBotChoiceViewModel>)(() =>
                    {
                        var result = new LinkageBotChoiceViewModel {
                            Name = "Firebot",
                        };

                        result.ActionChoices.AddRange([
                            ((Func<LinkageActionChoiceViewModel>)(() =>
                            {
                                var result = new LinkageActionChoiceViewModel{
                                    Name = "Mark Timestamp",
                                    Identifier = "8201e995-0070-4051-801f-7aced0118998"
                                };

                                result.Parameters.AddRange([
                                    new LinkageParameterViewModel {
                                        Name = "Purpose"
                                    },
                                    new LinkageParameterViewModel {
                                        Name = "Format"
                                    },
                                ]);

                                return result;
                            }))(),
                            ((Func<LinkageActionChoiceViewModel>)(() =>
                            {
                                var result = new LinkageActionChoiceViewModel{
                                    Name = "Disable Noisy Effects Queue",
                                    Identifier = "f727593f-0cf7-4a62-acc7-248af6b10900"
                                };

                                result.Parameters.AddRange([
                                    new LinkageParameterViewModel {
                                        Name = "Delay",
                                    },
                                    new LinkageParameterViewModel {
                                        Name = "Interrupt",
                                    },
                                ]);

                                return result;
                            }))(),
                            new LinkageActionChoiceViewModel{
                                Name = "Enable Noisy Effects Queue",
                                Identifier = "27831e4c-b946-4661-b857-2456c83b5647"
                            }
                        ]);

                        return result;
                    }))(),
                ]);

                result.SelectedBot = result.BotChoices[0];
                result.SelectedBotAction = result.SelectedBot.ActionChoices[0];
                result.SelectedMacroPad = result.MacroPadChoices[0];
                result.SelectedSwitch = result.SelectedMacroPad.SwitchChoices[0];
                result.SelectedSwitchState = ViewModels.SwitchState.On;

                result.Arguments.Add(new BotActionArgumentViewModel
                {
                    Name = result.SelectedBotAction.Parameters[0].Name,
                    Value = "Perigrin"
                });

                result.InvalidArguments.Add(new BotActionArgumentViewModel
                {
                    Name = "MissingNo",
                    Value = "Meriadoc"
                });

                return result;
            }
        }
    }
}