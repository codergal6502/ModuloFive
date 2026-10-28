using System.Diagnostics.CodeAnalysis;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.MacroPad.Serial;

namespace ModuloFivePadClient.RunnerCore
{
    public class SwitchToggleModeWrapper : ISwitchToggleModeWrapper, IDisposable
    {
        private struct SwitchIdentifier
        {
            // See https://stackoverflow.com/a/2975784/1102726.
            // See https://stackoverflow.com/a/2150233/1102726.
            // Also, neatly, the compiler makes you do both if you do Object.Equals!
            //     'SwitchToggleModeWrapper.ButtonIdentifier' overrides Object.Equals(object o) but does not override Object.GetHashCode()CS0659
            // See https://stackoverflow.com/a/2975784/1102726

            public Guid serialId;
            public byte switchNumber;

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is not SwitchIdentifier bi) return false;
                return Guid.Equals(bi.serialId, serialId) && byte.Equals(bi.switchNumber, switchNumber);
            }

            public override int GetHashCode()
            {
                // byte.GetHashCode seems to just be the byte value itself.
                return serialId.GetHashCode() + switchNumber.GetHashCode();
            }
        }

        private readonly List<IMacroPadConnection> _macroPadConnections;
        private readonly IModuloFivePadConfigurationProvider _configurationProvider;
        private readonly IMacroPadConnectionBuilder _macroPadConnectionBuilder;
        private readonly Dictionary<SwitchIdentifier, SwitchState> _toggleModeSwitchStates;
        private ModuloFiveConfiguration _cachedConfiguration;

        public SwitchToggleModeWrapper(IModuloFivePadConfigurationProvider configurationProvider, IMacroPadConnectionBuilder macroPadConnectionBuilder)
        {

            this._macroPadConnections = [];
            this._toggleModeSwitchStates = [];
            this._configurationProvider = configurationProvider;
            this._configurationProvider.ConfigurationChanged += UpdateModeMapping;
            this._cachedConfiguration = this._configurationProvider.CloneCurrentConfigurationCopy();

            this._macroPadConnectionBuilder = macroPadConnectionBuilder;
            this._macroPadConnectionBuilder.MacroPadConnected += AddConnectedMacroPad;

            UpdateModeMapping(this._cachedConfiguration);
        }

        private void AddConnectedMacroPad(IMacroPadConnectionBuilder sender, IMacroPadConnection macroPadConnection)
        {
            _macroPadConnections.Add(macroPadConnection);
            macroPadConnection.SwitchStateChanged += FowardSwitchStateChange;
        }

        private void UpdateModeMapping(ModuloFiveConfiguration configuration)
        {
            this._cachedConfiguration = configuration;

            foreach (var key in _toggleModeSwitchStates.Keys.ToList())
            {
                var padSwitch = configuration
                    .MacroPads
                    .FirstOrDefault(mp => mp.SerialId == key.serialId)
                    ?.Switches
                    ?.FirstOrDefault(s => s.SwitchNumber == key.switchNumber)
                    ?.SwitchMode;

                if (padSwitch != ModuloFiveConfiguration.SwitchMode.SimulatedToggle)
                {
                    _toggleModeSwitchStates.Remove(key);
                }
            }

            foreach (var pad in configuration.MacroPads)
            {
                foreach (var sw in pad.Switches)
                {
                    #warning remove commented out code
                    // if ((null != pad.SerialId) && (null != sw.SwitchNumber))
                    {
                        var key = new SwitchIdentifier { serialId = (Guid)pad.SerialId, switchNumber = (byte)sw.SwitchNumber };

                        switch (sw.SwitchMode)
                        {
                            case ModuloFiveConfiguration.SwitchMode.Momentary:
                            case ModuloFiveConfiguration.SwitchMode.PhysicalToggle:
                            default:
                                {
                                    // These two switch modes are functionally identical.
                                    _toggleModeSwitchStates.Remove(key);
                                    break;
                                }
                            case ModuloFiveConfiguration.SwitchMode.SimulatedToggle:
                                {
                                    if (!_toggleModeSwitchStates.ContainsKey(key))
                                    {
                                        _toggleModeSwitchStates[key] = SwitchState.Off;
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private void FowardSwitchStateChange(IMacroPadConnection macroPad, SwitchStateChangeMessage message)
        {
            var key = new SwitchIdentifier { serialId = macroPad.SerialId, switchNumber = message.SwitchNumber };
            if (_toggleModeSwitchStates.TryGetValue(key, out SwitchState currentToggleState))
            {
                SwitchState resultingState = currentToggleState;
                if (message.SwitchState == SwitchState.On)
                {
                    switch (currentToggleState)
                    {
                        case SwitchState.Off:
                        default:
                            {
                                resultingState = SwitchState.On;
                                break;
                            }
                        case SwitchState.On:
                            {
                                resultingState = SwitchState.Off;
                                break;
                            }
                    }

                    _toggleModeSwitchStates[key] = resultingState;
                    this.SwitchStateChanged?.Invoke(macroPad, new LogicalSwitchStateChangedArgs(resultingState, true, message));
                }
                else
                {
                    this.SwitchStateChanged?.Invoke(macroPad, new LogicalSwitchStateChangedArgs(resultingState, false, message));
                }
            }
            else
            {
                this.SwitchStateChanged?.Invoke(macroPad, new LogicalSwitchStateChangedArgs(message.SwitchState, true, message));
            }
        }

        public event LogicalSwitchStateChangedEvent? SwitchStateChanged;

        public void Dispose()
        {
            this._configurationProvider.ConfigurationChanged -= UpdateModeMapping;
            this._macroPadConnectionBuilder.MacroPadConnected -= AddConnectedMacroPad;
            foreach (var macroPadConnection in _macroPadConnections)
            {
                macroPadConnection.SwitchStateChanged += FowardSwitchStateChange;
            }
        }
    }
}
