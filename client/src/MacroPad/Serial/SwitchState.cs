namespace ModuloFivePadClient.MacroPad.Serial
{
    public enum SwitchState : byte
    {
        On = 0b10000000,
        Off = 0b00000000,
        Changed = 0xEE,
        Unknown = 0xFF,
    }
}