namespace ModuloFivePadClient.BotConnections
{
    [Serializable]
    public class BotConnectionConfigurationException : BotConnectionException
    {
        public BotConnectionConfigurationException(string? message) : base(message)
        {
        }

        public BotConnectionConfigurationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}