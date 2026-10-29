using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockObjects;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.MacroPad.Serial;
using ModuloFivePadClient.RunnerCore;

namespace RunnerCoreTests;

public sealed class RunnerCoreLinkageScenarios
{
    private static readonly ILogger<Runner> runnerLogger = new NullLogger<Runner>();

    // POSITIVE SCENARIOS:
    // A: Missing Components
    //    1. No Matching Bot
    //       No events raised/callbacks made
    //    2. No Matching Bot Action
    //       No events raised/callbacks made
    //    3. No Matching Macro Pad
    //       No events raised/callbacks made
    //    4. No Matching Switch Number
    //       No events raised/callbacks made
    // B: Bot Action Invocation Failed
    //    OnMockInvokeAction
    //    ActionFailed
    // C: Switches:
    //    1. Trigger = Changed, State = On
    //       OnMockInvokeAction
    //       ActionSuceeded
    //       LinkagePerformed
    //    2. Trigger = Changed, State = Off
    //       OnMockInvokeAction
    //       ActionSuceeded
    //       LinkagePerformed
    //    3. Trigger = On, State = On
    //       OnMockInvokeAction
    //       ActionSuceeded
    //       LinkagePerformed
    //    4. Trigger = On, State = Off
    //       No events raised/callbacks made
    //    5. Trigger = Off, State = On
    //       No events raised/callbacks made
    //    6. Trigger = Off, State = Off
    //       OnMockInvokeAction
    //       ActionSuceeded
    //       LinkagePerformed
    // D: Parameter/Argument Mismatch
    //    1. ????? pretty sure this is outside the parameterized thing

    [TestClass]
    public sealed class Positive
    {
        public void SwitchChanged_Parameterized(
            bool hasMatchingBot,
            bool hasMatchingBotAction,
            bool hasMatchingMacroPad,
            bool hasMatchingSwitchNumber,
            bool shouldInvocationSucceed,
            bool expectOnMockInvokeAction,
            bool expectActionSuceeded,
            bool expectLinkagePerformed,
            bool expectActionFailed,
            ModuloFiveConfiguration.SwitchState triggerState,
            ModuloFiveConfiguration.SwitchState switchState
        )
        {
            ModuloFiveConfiguration cfg = new ModuloFiveConfiguration();

            List<ModuloFiveConfiguration.SwitchState> triggerStatesToConfigure = [ModuloFiveConfiguration.SwitchState.On, ModuloFiveConfiguration.SwitchState.Off, ModuloFiveConfiguration.SwitchState.Changed];
            List<ModuloFiveConfiguration.SwitchState> switchStatesToSimulate = [ModuloFiveConfiguration.SwitchState.On, ModuloFiveConfiguration.SwitchState.Off];

            var debugNumber = 0;

            foreach (var t in triggerStatesToConfigure)
            {
                foreach (var s in switchStatesToSimulate)
                {
                    debugNumber++;

                    string guidString = $"00000000-0000-0000-0000-0000000000{debugNumber:d2}";
                    ModuloFiveConfiguration.Bot b;
                    ModuloFiveConfiguration.BotAction ba;

                    cfg.Bots.Add(
                        b = new ModuloFiveConfiguration.Bot
                        {
                            Name = $"bot number {debugNumber}",//MockTextGenerator.Generate(3),
                            Nickname = MockTextGenerator.Generate(3),
                            IsEnabled = true,
                            BotActions = [ ba = new ModuloFiveConfiguration.BotAction {
                                Name = MockTextGenerator.Generate(3),
                                BotActionId = guidString,
                                Parameters = [.. Enumerable.Range(0, 3).Select(_ =>
                                    new ModuloFiveConfiguration.BotActionParameter {
                                        Name = MockTextGenerator.Generate(3)
                                    }
                                )]
                            }]
                        }
                    );

                    ModuloFiveConfiguration.MacroPad mp;
                    cfg.MacroPads.Add(
                        mp = new ModuloFiveConfiguration.MacroPad
                        {
                            SerialId = Guid.Parse(guidString),
                            Nickname = MockTextGenerator.Generate(3),
                            ProductId = Guid.NewGuid(),
                            Version = new Version(6, 55, 35),
                            Switches = [.. Enumerable.Range(0, 5).Select(swNum =>
                                new ModuloFiveConfiguration.Switch {
                                    SwitchNumber = swNum,
                                    Nickname = MockTextGenerator.Generate(3),
                                    SwitchMode = ModuloFiveConfiguration.SwitchMode.Momentary
                                })
                            ]
                        }
                    );

                    cfg.Linkages.Add(
                        new ModuloFiveConfiguration.Linkage
                        {
                            BotActionId = ba.BotActionId,
                            BotActionName = ba.Name,
                            MacroPadSerialId = mp.SerialId,
                            BotName = b.Name,
                            SwitchNumber = mp.Switches.Shuffle().First().SwitchNumber,
                            TriggerState = t,
                            Arguments = [.. ba.Parameters.Select(p => new ModuloFiveConfiguration.LinkageArgument {
                                Name = p.Name!,
                                Value = MockTextGenerator.Generate(3)
                            })]
                        }
                    );
                }
            }

            MockSwitchToggleWrapper wrp = new();
            MockBotConnectionBuilder bld = new();

            List<MockBotConnection> allMockBotConnections = [..
                cfg.Bots.Select(b => new MockBotConnection {
                    BotName = b.Name!,
                    BotActions = [..b.BotActions.Select(ba => new ModuloFivePadClient.BotConnections.BotAction {
                        BotActionId = ba.BotActionId,
                        Name = ba.Name,
                        Parameters = [..ba.Parameters.Select(p => new ModuloFivePadClient.BotConnections.BotAction.Parameter {
                            Name = p.Name
                        })]
                    })],
                    ShouldInvocationSucceed = shouldInvocationSucceed
                })
            ];

            int actionInvokedCount = 0;
            int linkagePerformedCount = 0;
            int actionSucceededCount = 0;
            int actionFailedCount = 0;

            var linkagesMatchingSwitchStateParameters =
                cfg
                    .Linkages
                    .Where(l => l.TriggerState == triggerState);

            var arbitraryMatchingCfgLinkage = linkagesMatchingSwitchStateParameters.First();

            string botName;
            if (hasMatchingBot)
            {
                botName = arbitraryMatchingCfgLinkage.BotName;
            }
            else
            {
                botName = MockTextGenerator.Generate(3);
                arbitraryMatchingCfgLinkage.BotName = botName;
            }

            string botActionId;
            if (hasMatchingBotAction)
            {
                botActionId = arbitraryMatchingCfgLinkage.BotActionId;
            }
            else
            {
                botActionId = Guid.NewGuid().ToString();
                arbitraryMatchingCfgLinkage.BotActionId = botActionId;
            }

            Guid macroPadSerialId;
            if (hasMatchingMacroPad)
            {
                macroPadSerialId = arbitraryMatchingCfgLinkage.MacroPadSerialId;
            }
            else
            {
                macroPadSerialId = Guid.NewGuid();
                arbitraryMatchingCfgLinkage.MacroPadSerialId = macroPadSerialId;
            }

            int switchNumber = arbitraryMatchingCfgLinkage.SwitchNumber;

            // See https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/null-safety/nullable-reference-types#override-the-warnings-with-.
            var matchinCfgMacroPad = cfg.MacroPads.SingleOrDefault(mp => mp.SerialId == arbitraryMatchingCfgLinkage.MacroPadSerialId);
            MockMacroPadConnection mpConn = new()
            {
                MacroPadVersion = matchinCfgMacroPad?.Version ?? new Version(3, 27, 68),
                ProductId = matchinCfgMacroPad?.ProductId ?? Guid.NewGuid(),
                SerialId = matchinCfgMacroPad?.SerialId ?? Guid.NewGuid()
            };

            MockBotConnection? bConn = allMockBotConnections.SingleOrDefault(b => b.BotName == botName);

            if (null != bConn)
            {
                bConn.OnMockInvokeAction += (invocationBotActionId, invocationBotArguments) =>
                {
                    invocationBotActionId.Should().Be(botActionId);
                    invocationBotArguments.Should().BeEquivalentTo(arbitraryMatchingCfgLinkage.Arguments);
                    actionInvokedCount++;
                };

                bConn.ActionSuceeded += (src, invocationBotActionId) =>
                {
                    src.Should().Be(bConn);
                    invocationBotActionId.Should().Be(botActionId);
                    actionSucceededCount++;
                };

                bConn.ActionFailed += (src, invocationBotActionId, reason) =>
                {
                    src.Should().Be(bConn);
                    invocationBotActionId.Should().Be(botActionId);
                    actionFailedCount++;
                };
            }

            Runner runner = new(runnerLogger, wrp, [bld]);
            runner.LoadConfiguration(cfg);

            runner.LinkagePerformed += (src, args) =>
            {
                src.Should().Be(runner);

                args.Linkage.MacroPadSerialId.Should().Be(macroPadSerialId);
                args.Linkage.SwitchNumber.Should().Be(switchNumber);
                args.Linkage.TriggerState.Should().HaveSameNameAs(triggerState);
                args.Linkage.BotName.Should().Be(botName);
                args.Linkage.BotActionId.Should().Be(botActionId);
                args.Linkage.Arguments.Should().BeEquivalentTo(arbitraryMatchingCfgLinkage.Arguments);
                args.Linkage.Invalid.Should().Be(false);
                linkagePerformedCount++;
            };

            foreach (var b in allMockBotConnections)
            {
                bld.DoBotConnected(b);
            }

            wrp.DoSwitchStateChanged(mpConn, new LogicalSwitchStateChangedArgs(
                logicalSwitchState: switchState == ModuloFiveConfiguration.SwitchState.On ? SwitchState.On : switchState == ModuloFiveConfiguration.SwitchState.Off ? SwitchState.Off : throw new NotImplementedException(switchState.ToString()),
                logicalSwitchStateChanged: true,
                originalMessage: new SwitchStateChangeMessage(
                    messageSource: ModuloFivePadClient.MacroPad.Serial.Messages.MessageSource.Pad,
                    messageIdentifier: 1,
                    switchNumber: hasMatchingSwitchNumber ? (byte)switchNumber : (byte)99,
                    switchState: (SwitchState)triggerState
                )
            ));

            actionInvokedCount.Should().Be(expectOnMockInvokeAction ? 1 : 0);
            linkagePerformedCount.Should().Be(expectLinkagePerformed ? 1 : 0);
            actionSucceededCount.Should().Be(expectActionSuceeded ? 1 : 0);
            actionFailedCount.Should().Be(expectActionFailed ? 1 : 0);
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsChanged_SwitchStateIsOff_SuccessCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: true,
                expectActionSuceeded: true,
                expectLinkagePerformed: true,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.Changed,
                switchState: ModuloFiveConfiguration.SwitchState.Off
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsChanged_SwitchStateIsOn_SuccessCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: true,
                expectActionSuceeded: true,
                expectLinkagePerformed: true,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.Changed,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOn_SwitchStateIsOff_NoCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: false,
                expectActionSuceeded: false,
                expectLinkagePerformed: false,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.On,
                switchState: ModuloFiveConfiguration.SwitchState.Off
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOn_SwitchStateIsOn_SuccessCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: true,
                expectActionSuceeded: true,
                expectLinkagePerformed: true,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.On,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOff_SwitchStateIsOff_SuccessCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: true,
                expectActionSuceeded: true,
                expectLinkagePerformed: true,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.Off,
                switchState: ModuloFiveConfiguration.SwitchState.Off
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOff_SwitchStateIsOn_NoCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: false,
                expectActionSuceeded: false,
                expectLinkagePerformed: false,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.Off,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOn_SwitchStateIsOn_NoBotMatches_NoCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: false,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: false,
                expectActionSuceeded: false,
                expectLinkagePerformed: false,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.On,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOn_SwitchStateIsOn_NoBotActionMatches_NoCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: false,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: false,
                expectActionSuceeded: false,
                expectLinkagePerformed: false,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.On,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOn_SwitchStateIsOn_NoMacroPad_NoCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: false,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: false,
                expectActionSuceeded: false,
                expectLinkagePerformed: false,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.On,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }

        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOn_SwitchStateIsOn_NoSwitch_NoCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: false,
                shouldInvocationSucceed: true,
                expectOnMockInvokeAction: false,
                expectActionSuceeded: false,
                expectLinkagePerformed: false,
                expectActionFailed: false,
                triggerState: ModuloFiveConfiguration.SwitchState.On,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }



        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsOn_SwitchStateIsOn_InvocationFails_NoCallbacks()
        {
            SwitchChanged_Parameterized(
                hasMatchingBot: true,
                hasMatchingBotAction: true,
                hasMatchingMacroPad: true,
                hasMatchingSwitchNumber: true,
                shouldInvocationSucceed: false,
                expectOnMockInvokeAction: true,
                expectActionSuceeded: false,
                expectLinkagePerformed: false,
                expectActionFailed: true,
                triggerState: ModuloFiveConfiguration.SwitchState.On,
                switchState: ModuloFiveConfiguration.SwitchState.On
            );
        }




    }

    [TestClass]
    public sealed class Negative
    {

    }
}
