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
    //    2. No Matching Macro Pad
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

    [TestClass]
    public sealed class Positive
    {
        [TestMethod]
        public void SwitchChanged_LinkedTriggerIsChanged_BotActionExists_BotActionWasInvoked_LinkageEventWasRaised()
        {
            string botName = MockTextGenerator.Generate(3);
            string botActionName = MockTextGenerator.Generate(3);
            string botActionId = Guid.NewGuid().ToString();
            Guid macroPadSerialId = Guid.NewGuid();
            int switchNumber = 3;
            var triggerState = ModuloFiveConfiguration.SwitchState.On;

            ModuloFiveConfiguration cfg = new ModuloFiveConfiguration
            {
                Bots = [
                    new ModuloFiveConfiguration.Bot {
                        Name = botName,
                        Nickname = MockTextGenerator.Generate(3),
                        IsEnabled = true,
                        BotActions = [
                            new ModuloFiveConfiguration.BotAction {
                                Name = botActionName,
                                Identifier = botActionId,
                                Parameters = []
                            }
                        ]
                    }
                ],
                MacroPads = [
                    new ModuloFiveConfiguration.MacroPad {
                        SerialId = macroPadSerialId,
                        Nickname = MockTextGenerator.Generate(3),
                        ProductId = Guid.NewGuid(),
                        Version = new Version(6, 55, 35),
                        Switches = [
                            new ModuloFiveConfiguration.Switch {
                                SwitchNumber = switchNumber,
                                Nickname = MockTextGenerator.Generate(3),
                                SwitchMode = ModuloFiveConfiguration.SwitchMode.Momentary
                            }
                        ]
                    }
                ],
                Linkages = [
                    new ModuloFiveConfiguration.Linkage {
                    BotActionId = botActionId,
                        BotActionName = botActionName,
                        MacroPadSerialId = macroPadSerialId,
                        BotName = botName,
                        SwitchNumber = switchNumber,
                        TriggerState = triggerState,
                        Arguments = []
                    }
                ]
            };

            MockSwitchToggleWrapper wrp = new();
            MockBotConnectionBuilder bld = new();

            // See https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/null-safety/nullable-reference-types#override-the-warnings-with-.
            MockMacroPadConnection mpConn = new()
            {
                MacroPadVersion = cfg.MacroPads.First().Version!,
                ProductId = (Guid)cfg.MacroPads.First()?.ProductId!,
                SerialId = (Guid)cfg.MacroPads.First()?.SerialId!,
            };

            MockBotConnection bConn = new()
            {
                BotName = cfg.Bots.First().Name!,
                BotActions = [
                    new ModuloFivePadClient.BotConnections.BotAction {
                        Identifier = cfg.Bots.First().
                        Name = MockTextGenerator.Generate(3),
                        Parameters = []
                    }
                ]
            };

            bld.ActiveBotConnections = [bConn];

            Runner runner = new(runnerLogger, wrp, [bld]);
            runner.LoadConfiguration(cfg);

            bool actionInvoked = false;
            bool linkagePerformed = false;
            bool actionSucceeded = false;
            bool actionFailed = false;

            bConn.OnMockInvokeAction += (botActionId, botArguments) =>
            {
                botActionId.Should().Be(botActionId);
                actionInvoked = true;
            };

            runner.LinkagePerformed += (src, args) =>
            {
                src.Should().Be(runner);
                args.Linkage.MacroPadSerialId.Should().Be(macroPadSerialId);
                args.Linkage.SwitchNumber.Should().Be(switchNumber);
                args.Linkage.TriggerState.Should().HaveSameNameAs(triggerState);
                args.Linkage.BotName.Should().Be(botName);
                args.Linkage.BotActionId.Should().Be(botActionId);
                args.Linkage.Arguments.Should().BeEmpty();
                args.Linkage.Invalid.Should().Be(false);
                linkagePerformed = true;
            };

            bConn.ActionSuceeded += (src) =>
            {
                src.Should().Be(bConn);
                actionSucceeded = true;
            };

            bConn.ActionFailed += (src, reason) =>
            {
                actionFailed = true;
            };

            bld.DoBotConnected(bConn);

            wrp.DoSwitchStateChanged(mpConn, new LogicalSwitchStateChangedArgs(
                logicalSwitchState: SwitchState.On,
                logicalSwitchStateChanged: true,
                originalMessage: new SwitchStateChangeMessage(
                    messageSource: ModuloFivePadClient.MacroPad.Serial.Messages.MessageSource.Pad,
                    messageIdentifier: 1,
                    switchNumber: (byte)switchNumber,
                    switchState: (SwitchState)triggerState
                )
            ));

            actionInvoked.Should().BeTrue();
            linkagePerformed.Should().BeTrue();
            actionSucceeded.Should().BeTrue();
            actionFailed.Should().BeFalse();
        }
    }

    [TestClass]
    public sealed class Negative
    {

    }
}
