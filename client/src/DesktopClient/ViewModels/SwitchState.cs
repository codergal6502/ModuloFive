using Microsoft.OpenApi;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public enum SwitchState
    {
        [Display("Unknown")]
        Unknown,
        [Display("On")]
        On,
        [Display("Off")]
        Off,
        [Display("Changed")]
        Changed,
    }
}
