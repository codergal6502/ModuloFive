namespace ModuloFivePadClient.BotConnections
{
    public class BotConnectionNetworkException : BotConnectionException
    {
        public BotConnectionNetworkException(string? message) : base(message)
        {
        }

        public BotConnectionNetworkException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}