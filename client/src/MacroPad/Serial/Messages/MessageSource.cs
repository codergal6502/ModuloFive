namespace ModuloFivePadClient.MacroPad.Serial.Messages
{
    public enum MessageSource : byte
    {
        Client = 0b00000000,
        Pad = 0b10000000,
        Unknown = 0xFF,
    }
}
