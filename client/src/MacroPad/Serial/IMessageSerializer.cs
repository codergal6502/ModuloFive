using ModuloFivePadClient.MacroPad.Serial.Messages;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public interface IMessageSerializer
    {
        byte[] SerializeMessage(GenericMessage genericMessage);
    }
}
