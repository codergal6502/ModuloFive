namespace ModuloFivePadClient.MacroPad.Serial
{
    public static class PadConstants
    {
        public static Guid MACRO_PAD_PRODUCT_ID => Guid.Parse("4f396736-45a9-44bf-97f3-192b00747d65");

        /// <summary>
        /// Somewhat arbitrary-selected two-byte message type codes.
        /// </summary>
        public static class CommandCodes
        {
            /// <summary>
            /// There is no payload for heartbeat messages.
            /// </summary>
            public static readonly byte[] HEARTBEAT = [0xFF, 0xFF];

            /// <summary>
            /// There is no payload for identification acknowledgements.
            /// </summary>
            public static byte[] IDENTIFICATION_ACK => [0b10000000, 0b00000001];
        }

        /// <summary>
        /// Somewhat arbitrary-selected two-byte message type codes.
        /// </summary>
        public static class ResponseCodes
        {
            /// <summary>
            /// The switch changed payload contains a single byte following the type code.
            /// The first bit is the switch state. The last 5 bits are the switch number.
            /// </summary>
            public static byte[] SWITCH_CHANGED => [0xF0, 0x0F];
        }
    }
}