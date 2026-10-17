namespace ModuloFivePadClient.MacroPad.Serial.Messages
{
    public class GenericMessage
    {
        public GenericMessage(MessageSource messageSource, uint messageIdentifier)
        {
            MessageSource = messageSource;
            MessageIdentifier = messageIdentifier;

        }
        public MessageSource MessageSource { get; }
        public uint MessageIdentifier { get; }
    }
}