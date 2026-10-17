namespace ModuloFivePadClient.MacroPad.Serial.Messages
{
    public class FailedMessage : GenericMessage
    {
        public FailedMessage(string errorMessage, byte[]? bytes = null) : base(MessageSource.Unknown, default)
        {
            ErrorMessage = errorMessage;
            Bytes = bytes;
        }

        public static FailedMessage BadPayloadLength(int actualLength, MessageType messageType, int expectedLength)
        {
            return new FailedMessage($"Byte length {actualLength} is wrong for message type {messageType}; it should be {expectedLength}.");
        }

        public string ErrorMessage { get; }
        public byte[]? Bytes { get; }
    }
}
