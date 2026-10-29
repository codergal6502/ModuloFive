using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MockObjects;
using ModuloFivePadClient.Configuration;

namespace ConfigurationTests;

using ConcreteProvider = ModuleFivePadApplicationDataJsonConfigurationProvider;

public sealed class ConfigurationReadWrite
{
    static NullLogger<ConcreteProvider> providerLogger = new();

    [TestClass]
    public sealed class Positive
    {
        [TestMethod]
        public void CloneCurrentConfigurationCopy_NoConfigurationFileExists()
        {
            ModuloFiveConfiguration? cfgWritten = null;
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => false,
                MockWriteConfiguration = (cfg) => cfgWritten = cfg,
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);
            var cfgActual = provider.CloneCurrentConfigurationCopy();

            cfgActual.Bots.Should().BeEmpty();
            cfgActual.Linkages.Should().BeEmpty();
            cfgActual.MacroPads.Should().BeEmpty();
            cfgActual.IsFirstTimeUser.Should().BeTrue();
            cfgActual.ShowTechnicalInfo.Should().BeFalse();

            cfgActual.Should().BeEquivalentTo(cfgWritten);
        }

        [TestMethod]
        public void CloneCurrentConfigurationCopy_ConfigurationFileExists_ConfigurationFileIsPopulated()
        {
            var cfgExpected = new ModuloFiveConfiguration
            {
                IsFirstTimeUser = false,
                ShowTechnicalInfo = true,
                Bots = [ new ModuloFiveConfiguration.Bot {
                IsEnabled = true,
                Name = "Lorem ipsum dolor sit amet",
                Nickname = "Consectetur adipiscing elit",
                BotActions = [
                    new ModuloFiveConfiguration.BotAction {
                        BotActionId = "c9ea2ad5-4fcf-4338-a18d-96af78773837",
                        Name = "Sed do eiusmod tempor",
                        Parameters = [
                            new ModuloFiveConfiguration.BotActionParameter {
                                Name = "incididunt ut labore"
                            }
                        ]
                    }
                ]
            } ]
            };

            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => cfgExpected
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);
            var cfgActual = provider.CloneCurrentConfigurationCopy();

            cfgActual.Should().NotBeSameAs(cfgExpected);
            cfgActual.Should().BeEquivalentTo(cfgExpected);
        }

        [TestMethod]
        public void UpdateConfiguration_Immediate()
        {
            var cfgToPersist = new ModuloFiveConfiguration
            {
                IsFirstTimeUser = false,
                ShowTechnicalInfo = true,
                Bots = [ new ModuloFiveConfiguration.Bot {
                    IsEnabled = true,
                    Name = "Et dolore magna aliqua",
                    Nickname = "Ut enim ad minim veniam",
                    BotActions = [
                        new ModuloFiveConfiguration.BotAction {
                            BotActionId = "69513a79-70d8-4b6d-8728-a4ff36994e05",
                            Name = "Auis nostrud exercitation",
                            Parameters = [
                                new ModuloFiveConfiguration.BotActionParameter {
                                    Name = "ullamco laboris nisi"
                                }
                            ]
                        }
                    ]
                } ],
                MacroPads = [
                    new ModuloFiveConfiguration.MacroPad {
                        SerialId = new Guid("860b0b6a-f57e-43a7-addb-d0cd28dfad17"),
                        Version = new Version(6, 80, 50),
                        ProductId = new Guid("66f0193f-2f3d-4052-b276-2e4cfc4b11a1"),
                        Nickname = "Ut aliquip ex ea commodo consequat",
                        Switches = [
                            new ModuloFiveConfiguration.Switch {
                                Nickname = "Duis aute irure dolor in reprehenderit",
                                SwitchMode = ModuloFiveConfiguration.SwitchMode.Momentary,
                                SwitchNumber = 0
                            }
                        ]
                    }
                ]
            };

            ModuloFiveConfiguration? cfgWritten = null;

            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => false,
                MockWriteConfiguration = (cfg) => cfgWritten = cfg
            };

            ConcreteProvider cp = new(providerLogger, cfgFsSvc);

            cp.UpdateConfiguration(cfgToPersist);

            // Typically, don't test multiple things at once, in this case the
            // equivalences of the configuration saved to the configuration
            // argument, of the configuration cloned and the configuration
            // argument, and of the configuration saved and the configuration
            // cloned. Since three equivalences are tightly coupled, it's better
            // to have one test that tests all three (one implied by the other
            // two) than to have three nearly-identical tests.
            cfgWritten.Should().NotBeNull();
            cfgWritten.Should().BeEquivalentTo(cfgToPersist);
            cfgWritten.Should().BeEquivalentTo(cp.CloneCurrentConfigurationCopy());
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceBotNickname()
        {
            var cfgToPersist = new ModuloFiveConfiguration
            {
                IsFirstTimeUser = false,
                ShowTechnicalInfo = true,
                Bots = [ new ModuloFiveConfiguration.Bot {
                    IsEnabled = true,
                    Name = "Et dolore magna aliqua",
                    Nickname = "   \r \t   \n",
                } ],
            };

            ModuloFiveConfiguration? cfgWritten = null;

            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => false,
                MockWriteConfiguration = (cfg) => cfgWritten = cfg
            };

            ConcreteProvider cp = new(providerLogger, cfgFsSvc);

            cp.UpdateConfiguration(cfgToPersist);

            // Typically, don't test multiple things at once, in this case the
            // equivalences of the configuration saved to the configuration
            // argument, of the configuration cloned and the configuration
            // argument, and of the configuration saved and the configuration
            // cloned. Since three equivalences are tightly coupled, it's better
            // to have one test that tests all three (one implied by the other
            // two) than to have three nearly-identical tests.
            cfgWritten.Should().NotBeNull();
            cfgWritten.Bots.Should().HaveCount(1);
            cfgWritten.Bots.Single().Nickname.Should().BeNull();
        }


        [TestMethod]
        public void UpdateConfiguration_WhitespaceLinkageArgumentValue()
        {
            var botActionId = "95adbf5f-8f3e-4922-95a2-094805150f24";
            var botActionName = "Etiam rutrum nisl";
            var botName = "Etiam rutrum nisl";
            var macroPadSerialId = new Guid("eb5c5c36-7363-4b31-8182-9ea60b190395");
            var switchNumber = 1;
            var botActionParameterName = "Nam venenatis condimentum lectus";

            var cfgToPersist = new ModuloFiveConfiguration
            {
                IsFirstTimeUser = false,
                ShowTechnicalInfo = true,
                Bots = [ new ModuloFiveConfiguration.Bot {
                    IsEnabled = true,
                    Name = botName,
                    Nickname = "Ut enim ad minim veniam",
                    BotActions = [
                        new ModuloFiveConfiguration.BotAction {
                            BotActionId = botActionId,
                            Name = botActionName,
                            Parameters = [
                                new ModuloFiveConfiguration.BotActionParameter {
                                    Name = botActionParameterName
                                }
                            ]
                        }
                    ]
                } ],
                MacroPads = [
                    new ModuloFiveConfiguration.MacroPad {
                        SerialId = macroPadSerialId,
                        Version = new Version(6, 80, 50),
                        ProductId = new Guid("66f0193f-2f3d-4052-b276-2e4cfc4b11a1"),
                        Nickname = "Ut aliquip ex ea commodo consequat",
                        Switches = [
                            new ModuloFiveConfiguration.Switch {
                                Nickname = "Duis aute irure dolor in reprehenderit",
                                SwitchMode = ModuloFiveConfiguration.SwitchMode.Momentary,
                                SwitchNumber = switchNumber
                            }
                        ]
                    }
                ],
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                        BotActionId = botActionId,
                        BotActionName = botActionName,
                        BotName = botName,
                        MacroPadSerialId = macroPadSerialId,
                        SwitchNumber = switchNumber,
                        TriggerState = ModuloFiveConfiguration.SwitchState.On,
                        Arguments = [
                            new ModuloFiveConfiguration.LinkageArgument {
                                Name = botActionParameterName,
                                Value = "   \t  \n \r \t\t\r"
                            }
                        ]
                    }
                ]
            };

            ModuloFiveConfiguration? cfgWritten = null;

            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => false,
                MockWriteConfiguration = (cfg) => cfgWritten = cfg
            };

            ConcreteProvider cp = new(providerLogger, cfgFsSvc);

            cp.UpdateConfiguration(cfgToPersist);

            // Typically, don't test multiple things at once, in this case the
            // equivalences of the configuration saved to the configuration
            // argument, of the configuration cloned and the configuration
            // argument, and of the configuration saved and the configuration
            // cloned. Since three equivalences are tightly coupled, it's better
            // to have one test that tests all three (one implied by the other
            // two) than to have three nearly-identical tests.
            cfgWritten.Should().NotBeNull();
            cfgWritten.Linkages.Should().HaveCount(1);
            cfgWritten.Linkages.Single().Arguments.Should().HaveCount(1);
            cfgWritten.Linkages.Single().Arguments.Single().Value.Should().BeNull();
        }
    }

    [TestClass]
    public sealed class Negative
    {
        [TestMethod]
        public void UpdateConfiguration_NullArgument()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            provider
                .Invoking(cp => cp.UpdateConfiguration(null, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceBotName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Bots = [ new ModuloFiveConfiguration.Bot {
                    Name = "   \r  \t \n "
                } ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Bot)}.{nameof(ModuloFiveConfiguration.Bot.Name)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_NullBotName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Bots = [ new ModuloFiveConfiguration.Bot {
                    Name = null,
                } ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Bot)}.{nameof(ModuloFiveConfiguration.Bot.Name)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceActionId()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Bots = [ new ModuloFiveConfiguration.Bot {
                    Name = "In voluptate velit esse cillum dolore",
                    BotActions = [
                        new ModuloFiveConfiguration.BotAction {
                            BotActionId = "\r   \t\n \t",
                            Name = "eu fugiat nulla pariatur",
                        }
                    ]
                } ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.BotAction)}.{nameof(ModuloFiveConfiguration.BotAction.BotActionId)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_NullActionId()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Bots = [ new ModuloFiveConfiguration.Bot {
                    Name = "In voluptate velit esse cillum dolore",
                    BotActions = [
                        new ModuloFiveConfiguration.BotAction {
                            BotActionId = null,
                            Name = "eu fugiat nulla pariatur",
                        }
                    ]
                } ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.BotAction)}.{nameof(ModuloFiveConfiguration.BotAction.BotActionId)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceActionName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Bots = [ new ModuloFiveConfiguration.Bot {
                    Name = "In voluptate velit esse cillum dolore",
                    BotActions = [
                        new ModuloFiveConfiguration.BotAction {
                            BotActionId = "39623ab7-e579-4737-89d5-efbb37b9d52e",
                            Name = "\r   \t\n \t"
                        }
                    ]
                } ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.BotAction)}.{nameof(ModuloFiveConfiguration.BotAction.Name)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_NullActionName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Bots = [ new ModuloFiveConfiguration.Bot {
                    Name = "In voluptate velit esse cillum dolore",
                    BotActions = [
                        new ModuloFiveConfiguration.BotAction {
                            BotActionId = "11124c48-58ad-42c0-b01b-0703991aab01",
                            Name = null
                        }
                    ]
                } ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.BotAction)}.{nameof(ModuloFiveConfiguration.BotAction.Name)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceLinkageBotActionId()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                        BotActionId = "  \r\t\n  \r \n \t",
                        BotActionName = "Nam eget vestibulum justo",
                        BotName = "ut dictum dolor",
                        MacroPadSerialId = new Guid("f3cdaf1c-3203-4f50-ac81-96e924d4034d"),
                        SwitchNumber = 0,
                        TriggerState = ModuloFiveConfiguration.SwitchState.Changed,
                        Arguments = [],
                    }
                ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotActionId)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceLinkageBotActionName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                        BotActionId = "ba3b64fb-5457-45ea-993f-4b63e9b7f2a8",
                        BotActionName = "  \r\t   \n",
                        BotName = "ut dictum dolor",
                        MacroPadSerialId = new Guid("f3cdaf1c-3203-4f50-ac81-96e924d4034d"),
                        SwitchNumber = 0,
                        TriggerState = ModuloFiveConfiguration.SwitchState.Changed,
                        Arguments = [],
                    }
                ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotActionName)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_NullLinkageBotActionName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                        BotActionId = "ba3b64fb-5457-45ea-993f-4b63e9b7f2a8",
                        BotActionName = null,
                        BotName = "ut dictum dolor",
                        MacroPadSerialId = new Guid("f3cdaf1c-3203-4f50-ac81-96e924d4034d"),
                        SwitchNumber = 0,
                        TriggerState = ModuloFiveConfiguration.SwitchState.Changed,
                        Arguments = []
                    }
                ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotActionName)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceLinkageBotName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                        BotActionId = "ba3b64fb-5457-45ea-993f-4b63e9b7f2a8",
                        BotActionName = "ut dictum dolor",
                        BotName = "  \r\t   \n",
                        MacroPadSerialId = new Guid("f3cdaf1c-3203-4f50-ac81-96e924d4034d"),
                        SwitchNumber = 0,
                        TriggerState = ModuloFiveConfiguration.SwitchState.Changed,
                        Arguments = [],
                    }
                ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotName)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_NullLinkageBotName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                        BotActionId = "ba3b64fb-5457-45ea-993f-4b63e9b7f2a8",
                        BotActionName = "ut dictum dolor",
                        BotName = null,
                        MacroPadSerialId = new Guid("f3cdaf1c-3203-4f50-ac81-96e924d4034d"),
                        SwitchNumber = 0,
                        TriggerState = ModuloFiveConfiguration.SwitchState.Changed,
                        Arguments = [],
                    }
                ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.Linkage)}.{nameof(ModuloFiveConfiguration.Linkage.BotName)} is invalid.");
        }

        [TestMethod]
        public void UpdateConfiguration_WhitespaceLinkageArgumentName()
        {
            var cfgFsSvc = new MockConfigurationFileSystemService
            {
                MockConfigurationFileExists = () => true,
                MockReadConfiguration = () => new ModuloFiveConfiguration()
            };

            ConcreteProvider provider = new(providerLogger, cfgFsSvc);

            var invalidCfg = new ModuloFiveConfiguration
            {
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                        BotActionId = "ba3b64fb-5457-45ea-993f-4b63e9b7f2a8",
                        BotActionName = "ut dictum dolor",
                        BotName = "In hac habitasse platea dictumst",
                        MacroPadSerialId = new Guid("95f94d94-f978-4bff-bf90-c936a46b2ddd"),
                        SwitchNumber = 1,
                        TriggerState = ModuloFiveConfiguration.SwitchState.Off,
                        Arguments = [
                            new ModuloFiveConfiguration.LinkageArgument {
                                Name = "   \r \t    \n\t\r\n  ",
                                Value = "Nam dapibus, magna at porta scelerisque"
                            }
                        ]
                    }
                ]
            };

            provider
                .Invoking(cp => cp.UpdateConfiguration(invalidCfg, ConfigurationPersistenceMode.Immediate))
                .Should().Throw<ModuloFiveConfigurationException>().WithMessage($"Configuration with null, empty, or whitespace {nameof(ModuloFiveConfiguration.LinkageArgument)}.{nameof(ModuloFiveConfiguration.LinkageArgument.Name)} is invalid.");
        }
    }
}