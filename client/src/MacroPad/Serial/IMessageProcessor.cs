using ModuloFivePadClient.MacroPad.Serial.Messages;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public delegate void MessageReceivedEvent(GenericMessage message);

    public interface IMessageProcessor
    {
        void AddBytes(IList<byte> bytes);
        event MessageReceivedEvent? MessageReceivedEvent;
    }
}
