namespace ModuloFivePadClient.MacroPad.Serial.Messages
{
    public class AcknowledgementMessage : GenericMessage
    {
        public AcknowledgementMessage(MessageSource messageSource, uint messageIdentifier) : base(messageSource, messageIdentifier)
        {
        }
    }
}
