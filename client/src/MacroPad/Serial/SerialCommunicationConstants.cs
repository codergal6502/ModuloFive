namespace ModuloFivePadClient.MacroPad.Serial
{
    public static class SerialCommunicationConstants
    {
        public const byte FRAME_BEGIN = 0x1D;
        public const byte FRAME_END = 0xD1;
        public const byte FRAME_ESCAPE = 0x1B;
        public const byte FRAME_ESCAPE_XOR = 0x20;
        public const byte INITIATOR_MASK = 0b10000000;
        public const byte TYPE_MASK = 0b01111111;
    }
}
