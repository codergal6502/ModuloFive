using Microsoft.Extensions.Logging;
using ModuloFivePadClient.MacroPad.Serial.Messages;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public class MessageSerializer : IMessageSerializer
    {
        private ILogger<MessageSerializer> _logger;

        public MessageSerializer(ILogger<MessageSerializer> logger)
        {
            this._logger = logger;
        }

        public byte[] SerializeMessage(GenericMessage genericMessage)
        {
            return [.. GenerateBytes(genericMessage)];
        }

        private IEnumerable<byte> GenerateBytes(GenericMessage genericMessage)
        {
            yield return SerialCommunicationConstants.FRAME_BEGIN;

            switch (genericMessage)
            {
                case AcknowledgementMessage acknowledgementMessage:
                    {
                        foreach (byte b in GenerateAcknowledgementBytes(acknowledgementMessage))
                        {
                            yield return b;
                        }
                        break;
                    }
            }

            yield return SerialCommunicationConstants.FRAME_END;
        }

        private IEnumerable<byte> GenerateAcknowledgementBytes(AcknowledgementMessage acknowledgementMessage)
        {
            byte firstByte = (byte)(acknowledgementMessage.MessageIdentifier >> 24);
            firstByte |= (byte)acknowledgementMessage.MessageSource;
            yield return (byte)MessageType.Acknowledgement;
            byte[] identifierBytes =
            [
                firstByte,
                (byte)(acknowledgementMessage.MessageIdentifier >> 16),
                (byte)(acknowledgementMessage.MessageIdentifier >> 8),
                (byte)acknowledgementMessage.MessageIdentifier,
            ];

            foreach (var b in EscapeBytes(identifierBytes))
            {
                yield return b;
            }
        }

        private IEnumerable<byte> EscapeBytes(IEnumerable<byte> bytes)
        {
            foreach (var b in bytes)
            {
                switch (b)
                {
                    case SerialCommunicationConstants.FRAME_BEGIN:
                    case SerialCommunicationConstants.FRAME_ESCAPE:
                        {
                            yield return SerialCommunicationConstants.FRAME_ESCAPE;
                            yield return (byte)(b ^ SerialCommunicationConstants.FRAME_ESCAPE_XOR);
                            break;
                        }
                    default:
                        {
                            yield return b;
                            break;
                        }
                }
            }
        }
    }
}
