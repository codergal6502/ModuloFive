namespace ModuloFivePadClient.MacroPad.Serial.Messages
{
    public enum MessageType : byte
    {
        Identification = 0x99,
        // todo: create 
        AsciiLog = 0xEE,
        Acknowledgement = 0x88,
        SwitchStateChange = 0x77,
        SnoozeButtonEvents = 0x66,
    }
}
