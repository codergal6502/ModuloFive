using ModuloFivePadClient.Configuration;

namespace MockObjects;

public class MockConfigurationProvider : IModuloFivePadConfigurationProvider
{
    public ModuloFiveConfiguration MockConfiguration { get; set; } = new ModuloFiveConfiguration();

    public ConfigurationChangeEvent? ConfigurationChanged { get; set; }
    public TimeSpan WriteBufferRateLimit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ModuloFiveConfiguration CloneCurrentConfigurationCopy()
    {
        #warning move this to somewhere actually usable (this is better that the current live implementation) and unit test it.
        return new ModuloFiveConfiguration
        {
            IsFirstTimeUser = MockConfiguration.IsFirstTimeUser,
            ShowTechnicalInfo = MockConfiguration.ShowTechnicalInfo,
            Bots = [..MockConfiguration.Bots.Select(b =>
                new ModuloFiveConfiguration.Bot
                {
                    IsEnabled = b.IsEnabled,
                    Name = b.Name,
                    Nickname = b.Nickname,
                    BotActions = [..b.BotActions.Select(ba => new ModuloFiveConfiguration.BotAction {
                        Identifier = ba.Identifier,
                        Name = ba.Name,
                        Parameters = [..ba.Parameters.Select(p => new ModuloFiveConfiguration.BotActionParameter {
                            Name = p.Name,
                            Type = p.Type
                        })]
                    })]
                }
            )],
            MacroPads = [..MockConfiguration.MacroPads.Select(mp =>
                new ModuloFiveConfiguration.MacroPad {
                    Nickname = mp.Nickname,
                    ProductId = mp.ProductId,
                    SerialId = mp.SerialId,
                    Version = mp.Version,
                    Switches = [..mp.Switches.Select(sw => new ModuloFiveConfiguration.Switch {
                        Nickname = sw.Nickname,
                        SwitchMode = sw.SwitchMode,
                        SwitchNumber = sw.SwitchNumber
                    })]
                }
            )],
            Linkages = [..MockConfiguration.Linkages.Select(l =>
                new ModuloFiveConfiguration.Linkage {
                    BotActionId = l.BotActionId,
                    BotActionName = l.BotActionName,
                    BotName = l.BotName,
                    MacroPadSerialId = l.MacroPadSerialId,
                    SwitchNumber = l.SwitchNumber,
                    TriggerState = l.TriggerState,
                    Arguments = [..l.Arguments.Select(a => new ModuloFiveConfiguration.LinkageArgument {
                        Name = a.Name,
                        Value = a.Value
                    })]
                }
            )],
        };
    }

    public void UpdateConfiguration(ModuloFiveConfiguration configuration, ConfigurationPersistenceMode configurationPersistenceMode = ConfigurationPersistenceMode.Immediate)
    {
        throw new NotImplementedException();
    }

    public void WriteConfiguration()
    {
        throw new NotImplementedException();
    }
}
