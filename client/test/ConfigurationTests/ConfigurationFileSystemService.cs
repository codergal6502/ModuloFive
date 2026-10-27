
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockObjects;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.Configuration.Json;

namespace ConfigurationTests.Integration;

using static ModuloFivePadClient.Configuration.ModuloFiveConfiguration;
using static ModuloFivePadClient.Configuration.ModuloFiveConfigurationFileSystemService;
using ConcreteProvider = ModuleFivePadApplicationDataJsonConfigurationProvider;

public sealed class ConfigurationFileSystemService
{
    private static PathAndFileNameSet GenerateTempNames()
    {
        return new PathAndFileNameSet(
            preferencesDirectory: Directory.CreateTempSubdirectory(DateTime.Now.ToString("yyyyMMddHHmmssfff")).FullName,
            configurationFileName: "test-modulo-five-config.json"
        );
    }

    private static readonly ILogger<ModuloFiveConfigurationFileSystemService> logger = new NullLogger<ModuloFiveConfigurationFileSystemService>();

    [TestClass]
    public sealed class Positive
    {
        [TestMethod]
        public void ConfigurationFileExists_No()
        {
            var tempNames = GenerateTempNames();

            var cfgFsSvc = new ModuloFiveConfigurationFileSystemService(logger, tempNames);

            cfgFsSvc.ConfigurationFileExists().Should().BeFalse();
        }

        [TestMethod]
        public void ConfigurationFileExists_Yes()
        {
            var tempNames = GenerateTempNames();

            var jsonPath = Path.Join(tempNames.PreferencesDirectory, tempNames.ConfigurationFileName);
            using (FileStream fs = new(jsonPath, FileMode.Create))
            {
                using var tw = new StreamWriter(fs);
                tw.Write(
"""
{
    "isFirstTimeUser": true,
    "macroPads": [],
    "showTechnicalInfo": false,
    "bots": [],
    "linkages": []
}
"""
                );
            }

            var cfgFsSvc = new ModuloFiveConfigurationFileSystemService(logger, tempNames);

            cfgFsSvc.ConfigurationFileExists().Should().BeTrue();
        }

        [TestMethod]
        public void ReadConfiguration_FileExists()
        {
            Random r = new();
            Guid productId = Guid.NewGuid();
            Version version = new Version(r.Next(), r.Next(), r.Next());
            Guid serialId = Guid.NewGuid();
            string macroPadNickname = MockTextGenerator.Generate(10);
            byte switchNumber1 = (byte)(r.Next() % byte.MaxValue);
            string switchName1 = MockTextGenerator.Generate(10);
            SwitchMode switchMode1 = Enum.GetValues<SwitchMode>().OrderBy(sm => r.Next()).First();
            byte switchNumber2 = (byte)(r.Next() % byte.MaxValue);
            string switchName2 = MockTextGenerator.Generate(10);
            SwitchMode switchMode2 = Enum.GetValues<SwitchMode>().OrderBy(sm => r.Next()).First();
            bool showTechnicalInfo = r.Next() % 2 == 0;
            string botName = MockTextGenerator.Generate(10);
            string botNickname = MockTextGenerator.Generate(10);
            string botActionId1 = Guid.NewGuid().ToString();
            string botActionName1 = MockTextGenerator.Generate(10);
            string botActionId2 = Guid.NewGuid().ToString();
            string botActionName2 = MockTextGenerator.Generate(10);
            string botParameters2a = MockTextGenerator.Generate(1);
            string botParameters2b = MockTextGenerator.Generate(1);
            string botParameters2c = MockTextGenerator.Generate(1);
            bool botIsEnabled = r.Next() % 2 == 0;
            SwitchState linkageTriggerState1 = Enum.GetValues<SwitchState>().OrderBy(st => r.Next()).First();
            SwitchState linkageTriggerState2 = Enum.GetValues<SwitchState>().OrderBy(st => r.Next()).First();
            string linkageArgument2a = MockTextGenerator.Generate(3);
            string linkageArgument2b = MockTextGenerator.Generate(3);

            ModuloFiveConfiguration cfgExpected = new ModuloFiveConfiguration
            {
                IsFirstTimeUser = true,
                MacroPads = [
                new ModuloFiveConfiguration.MacroPad {
                        ProductId = productId,
                        Version = version,
                        SerialId = serialId,
                        Nickname = macroPadNickname,
                        Switches = [
                            new ModuloFiveConfiguration.Switch {
                                SwitchNumber =  switchNumber1,
                                Nickname =  switchName1,
                                SwitchMode =  switchMode1
                            },
                            new ModuloFiveConfiguration.Switch{
                                SwitchNumber = switchNumber2,
                                Nickname = switchName2,
                                SwitchMode = switchMode2,
                            }
                        ]
                    }
                ],
                ShowTechnicalInfo = showTechnicalInfo,
                Bots = [
                    new ModuloFiveConfiguration.Bot {
                        Name     =  botName,
                        Nickname     =  botNickname,
                        BotActions     =  [
                            new ModuloFiveConfiguration.BotAction {
                                Name =  botActionName1,
                                Identifier =  botActionId1,
                                Parameters =  []
                            },
                            new ModuloFiveConfiguration.BotAction {
                                Name = botActionName2,
                                Identifier = botActionId2,
                                Parameters = [
                                    new ModuloFiveConfiguration.BotActionParameter {
                                    Name = botParameters2a,
                                    },
                                    new ModuloFiveConfiguration.BotActionParameter {
                                    Name = botParameters2b,
                                    },
                                ]
                            }
                        ],
                        IsEnabled = botIsEnabled
                    }
                ],
                Linkages = [
                    new ModuloFiveConfiguration.Linkage
                    {
                        MacroPadSerialId = serialId,
                        SwitchNumber = switchNumber1,
                        TriggerState = linkageTriggerState1,
                        BotName = botName,
                        BotActionId = botActionId1,
                        BotActionName = botActionName1,
                        Arguments = []
                    },
                    new ModuloFiveConfiguration.Linkage
                    {
                        MacroPadSerialId =            serialId,
                        SwitchNumber =             switchNumber2,
                        TriggerState =             linkageTriggerState2,
                        BotName =             botName,
                        BotActionId =             botActionId2,
                        BotActionName =             botActionName2,
                        Arguments =             [
                            new ModuloFiveConfiguration.LinkageArgument {
                                Name = botParameters2a, Value = linkageArgument2a
                            },
                            new ModuloFiveConfiguration.LinkageArgument {
                                Name = botParameters2b, Value = linkageArgument2b
                            },
                        ]
                    }
                ]
            };

            var tempNames = GenerateTempNames();

            using (var fs = new FileStream(tempNames.ConfigurationFilePath, FileMode.CreateNew))
            {
                using StreamWriter sw = new(fs);
                sw.Write(JsonSerializer.Serialize(cfgExpected));
            }

            var cfgFsSvc = new ModuloFiveConfigurationFileSystemService(logger, tempNames);
            var cfgActual = cfgFsSvc.ReadConfiguration();

            cfgActual.Should().BeEquivalentTo(cfgExpected);
        }

        [TestMethod]
        public void WriteConfiguration()
        {
            Random r = new();
            Guid productId = Guid.NewGuid();
            Version version = new Version(r.Next(), r.Next(), r.Next());
            Guid serialId = Guid.NewGuid();
            string macroPadNickname = MockTextGenerator.Generate(10);
            byte switchNumber1 = (byte)(r.Next() % byte.MaxValue);
            string switchName1 = MockTextGenerator.Generate(10);
            SwitchMode switchMode1 = Enum.GetValues<SwitchMode>().OrderBy(sm => r.Next()).First();
            byte switchNumber2 = (byte)(r.Next() % byte.MaxValue);
            string switchName2 = MockTextGenerator.Generate(10);
            SwitchMode switchMode2 = Enum.GetValues<SwitchMode>().OrderBy(sm => r.Next()).First();
            bool showTechnicalInfo = r.Next() % 2 == 0;
            string botName = MockTextGenerator.Generate(10);
            string botNickname = MockTextGenerator.Generate(10);
            string botActionId1 = Guid.NewGuid().ToString();
            string botActionName1 = MockTextGenerator.Generate(10);
            string botActionId2 = Guid.NewGuid().ToString();
            string botActionName2 = MockTextGenerator.Generate(10);
            string botParameters2a = MockTextGenerator.Generate(1);
            string botParameters2b = MockTextGenerator.Generate(1);
            string botParameters2c = MockTextGenerator.Generate(1);
            bool botIsEnabled = r.Next() % 2 == 0;
            SwitchState linkageTriggerState1 = Enum.GetValues<SwitchState>().OrderBy(st => r.Next()).First();
            SwitchState linkageTriggerState2 = Enum.GetValues<SwitchState>().OrderBy(st => r.Next()).First();
            string linkageArgument2a = MockTextGenerator.Generate(3);
            string linkageArgument2b = MockTextGenerator.Generate(3);

            ModuloFiveConfiguration cfgExpected = new ModuloFiveConfiguration
            {
                IsFirstTimeUser = true,
                MacroPads = [
                new ModuloFiveConfiguration.MacroPad {
                        ProductId = productId,
                        Version = version,
                        SerialId = serialId,
                        Nickname = macroPadNickname,
                        Switches = [
                            new ModuloFiveConfiguration.Switch {
                                SwitchNumber =  switchNumber1,
                                Nickname =  switchName1,
                                SwitchMode =  switchMode1
                            },
                            new ModuloFiveConfiguration.Switch{
                                SwitchNumber = switchNumber2,
                                Nickname = switchName2,
                                SwitchMode = switchMode2,
                            }
                        ]
                    }
                ],
                ShowTechnicalInfo = showTechnicalInfo,
                Bots = [
                    new ModuloFiveConfiguration.Bot {
                        Name     =  botName,
                        Nickname     =  botNickname,
                        BotActions     =  [
                            new ModuloFiveConfiguration.BotAction {
                                Name =  botActionName1,
                                Identifier =  botActionId1,
                                Parameters =  []
                            },
                            new ModuloFiveConfiguration.BotAction {
                                Name = botActionName2,
                                Identifier = botActionId2,
                                Parameters = [
                                    new ModuloFiveConfiguration.BotActionParameter {
                                    Name = botParameters2a,
                                    },
                                    new ModuloFiveConfiguration.BotActionParameter {
                                    Name = botParameters2b,
                                    },
                                ]
                            }
                        ],
                        IsEnabled = botIsEnabled
                    }
                ],
                Linkages = [
                    new ModuloFiveConfiguration.Linkage
                    {
                        MacroPadSerialId = serialId,
                        SwitchNumber = switchNumber1,
                        TriggerState = linkageTriggerState1,
                        BotName = botName,
                        BotActionId = botActionId1,
                        BotActionName = botActionName1,
                        Arguments = []
                    },
                    new ModuloFiveConfiguration.Linkage
                    {
                        MacroPadSerialId =            serialId,
                        SwitchNumber =             switchNumber2,
                        TriggerState =             linkageTriggerState2,
                        BotName =             botName,
                        BotActionId =             botActionId2,
                        BotActionName =             botActionName2,
                        Arguments =             [
                            new ModuloFiveConfiguration.LinkageArgument {
                                Name = botParameters2a, Value = linkageArgument2a
                            },
                            new ModuloFiveConfiguration.LinkageArgument {
                                Name = botParameters2b, Value = linkageArgument2b
                            },
                        ]
                    }
                ]
            };

            var tempNames = GenerateTempNames();

            var cfgFsSvc = new ModuloFiveConfigurationFileSystemService(logger, tempNames);
            cfgFsSvc.WriteConfiguration(cfgExpected);

            using var fs = new FileStream(tempNames.ConfigurationFilePath, FileMode.Open);
            var cfgActual = JsonSerializer.Deserialize<ModuloFiveConfiguration>(fs, ModuloFiveConfigurationJsonContext.Default.ModuloFiveConfiguration);
            cfgActual.Should().BeEquivalentTo(cfgExpected);
        }
    }

    [TestClass]
    public sealed class Negative
    {
        [TestMethod]
        public void ReadConfiguration_FileDoesNotExists()
        {
            var tempNames = GenerateTempNames();

            var cfgFsSvc = new ModuloFiveConfigurationFileSystemService(logger, tempNames);
            cfgFsSvc
                .Invoking(s => s.ReadConfiguration())
                .Should()
                .Throw<ModuloFiveConfigurationException>()
                .WithMessage($"File {tempNames.ConfigurationFilePath} does not exist.");
        }

        [TestMethod]
        public void ReadConfiguration_FileContainsMagicWordNull()
        {
            var tempNames = GenerateTempNames();

            using (var fs = new FileStream(tempNames.ConfigurationFilePath, FileMode.CreateNew))
            {
                using StreamWriter sw = new(fs);
                sw.Write("null");
            }

            var cfgFsSvc = new ModuloFiveConfigurationFileSystemService(logger, tempNames);
            cfgFsSvc
                .Invoking(s => s.ReadConfiguration())
                .Should()
                .Throw<ModuloFiveConfigurationException>()
                .WithMessage($"File {tempNames.ConfigurationFilePath} contained the null object.");
        }

        [TestMethod]
        public void ReadConfiguration_BadJson()
        {
            var tempNames = GenerateTempNames();

            using (var fs = new FileStream(tempNames.ConfigurationFilePath, FileMode.CreateNew))
            {
                using StreamWriter sw = new(fs);
                sw.Write("{ 2jd;3 sdv9 3{  {{ a]] /'}380u4(*7@#$&*()");
            }

            var cfgFsSvc = new ModuloFiveConfigurationFileSystemService(logger, tempNames);
            cfgFsSvc
                .Invoking(s => s.ReadConfiguration())
                .Should()
                .Throw<ModuloFiveConfigurationException>()
                .WithMessage($"File {tempNames.ConfigurationFilePath} contained invalid JSON.");
        }
    }
}