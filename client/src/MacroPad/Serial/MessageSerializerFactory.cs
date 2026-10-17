using Microsoft.Extensions.Logging;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public class MessageSerializerFactory : IMessageSerializerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public MessageSerializerFactory(ILoggerFactory loggerFactory)
        {
            this._loggerFactory = loggerFactory;
        }

        public IMessageSerializer BuildMessageSerializer()
        {
            return new MessageSerializer(_loggerFactory.CreateLogger<MessageSerializer>());
        }
 
    }
}