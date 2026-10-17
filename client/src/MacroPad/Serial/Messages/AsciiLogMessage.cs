namespace ModuloFivePadClient.MacroPad.Serial.Messages
{
    public class AsciiLogMessage : GenericMessage
    {
        public AsciiLogMessage(MessageSource messageSource, uint messageIdentifier, string logMessage) : base(messageSource, messageIdentifier)
        {
            LogMessage = logMessage;
        }

        public string LogMessage { get; }
    }
}