using System;
using System.Collections.Generic;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class EnumSources
    {
        public static IEnumerable<SwitchState> SwitchStates { get => Enum.GetValues<SwitchState>(); }
        public static IEnumerable<SwitchMode> SwitchModes { get => Enum.GetValues<SwitchMode>(); }
    }
}