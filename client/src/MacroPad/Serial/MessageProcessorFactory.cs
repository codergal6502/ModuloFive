using Microsoft.Extensions.Logging;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public MessageProcessorFactory(ILoggerFactory loggerFactory)
        {
            this._loggerFactory = loggerFactory;
        }

        public IMessageProcessor BuildMessageProcessor()
        {
            return new MessageProcessor(_loggerFactory.CreateLogger<MessageProcessor>());
        }
    }
}