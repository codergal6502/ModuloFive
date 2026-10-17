namespace ModuloFivePadClient.MacroPad.Serial.Messages
{
    public class IdentificationMessage : GenericMessage
    {
        public IdentificationMessage(MessageSource messageSource, uint messageIdentifier, Guid productId, Version macroPadVersion, Guid serialId) : base(messageSource, messageIdentifier)
        {
            ProductId = productId;
            MacroPadVersion = macroPadVersion;
            SerialId = serialId;
        }

        public Guid ProductId { get; }
        public Version MacroPadVersion { get; }
        public Guid SerialId { get; }
    }
}
