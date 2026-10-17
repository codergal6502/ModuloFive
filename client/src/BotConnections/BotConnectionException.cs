namespace ModuloFivePadClient.BotConnections
{
    public abstract class BotConnectionException : Exception
    {
        public BotConnectionException(string? message) : base(message)
        {
        }

        public BotConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}