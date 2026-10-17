using Microsoft.OpenApi;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public enum SwitchMode
    {
        [Display("Momentary")]
        Momentary,
        [Display("Simulated Toggle")]
        SimulatedToggle,
        [Display("Physical Toggle")]
        PhysicalToggle
    }
}
