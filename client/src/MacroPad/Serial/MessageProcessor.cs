using Microsoft.Extensions.Logging;
using ModuloFivePadClient.MacroPad.Serial.Messages;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public class MessageProcessor : IMessageProcessor
    {
        private enum ParserState
        {
            WaitingForBegin,
            InFrame,
        }

        private ParserState _parserState;
        private List<byte>? _currentFrame;
        private readonly ILogger<MessageProcessor> _logger;

        public MessageProcessor(Microsoft.Extensions.Logging.ILogger<MessageProcessor> logger)
        {
            this._parserState = ParserState.WaitingForBegin;
            this._currentFrame = null;
            this._logger = logger;
        }
        public event MessageReceivedEvent? MessageReceivedEvent;

        private IEnumerable<byte> UnescapeBytes(byte[] bytes)
        {
            // See https://stackoverflow.com/a/1272677.
            IEnumerator<byte> enumerator = ((IEnumerable<byte>)bytes).GetEnumerator();

            while (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case SerialCommunicationConstants.FRAME_ESCAPE:
                        {
                            if (enumerator.MoveNext())
                            {
                                yield return (byte)(enumerator.Current ^ SerialCommunicationConstants.FRAME_ESCAPE_XOR);
                                break;
                            }
                            else
                            {
                                yield break;
                            }
                        }
                    default:
                        {
                            yield return enumerator.Current;
                            break;
                        }
                }
            }
        }

        public GenericMessage ParseMessage(IList<byte> frameBytes)
        {
            if (frameBytes.Count < 6)
            {
                return new FailedMessage($"Message is only {frameBytes.Count} bytes long but must be at least 6 bytes long.", [.. frameBytes]);
            }

            if (frameBytes[0] != SerialCommunicationConstants.FRAME_BEGIN)
            {
                return new FailedMessage($"Message started with 0x{frameBytes[0]:x2} instead of 0x{SerialCommunicationConstants.FRAME_BEGIN:x2}", [.. frameBytes]);
            }

            if (frameBytes.Last() != SerialCommunicationConstants.FRAME_END)
            {
                return new FailedMessage($"Message ended with 0x{frameBytes.Last():x2} instead of 0x{SerialCommunicationConstants.FRAME_END:x2}", [.. frameBytes]);
            }

            byte messageTypeCodeByte = frameBytes[1];
            if (!Enum.IsDefined(typeof(MessageType), messageTypeCodeByte))
            {
                return new FailedMessage($"Received unexpected message type code 0x{messageTypeCodeByte:x2}", [.. frameBytes]);
            }
            MessageType messageType = (MessageType)messageTypeCodeByte;

            var unesecaptedBytes = UnescapeBytes([.. frameBytes.Skip(2)]).ToArray();

            if (unesecaptedBytes.Length < 4)
            {
                return new FailedMessage($"Message after frame start marker and frame type is only {unesecaptedBytes.Length} bytes long but must be at least 4.", [.. frameBytes]);
            }

            byte messageSourceByte = (byte)(unesecaptedBytes[0] & SerialCommunicationConstants.INITIATOR_MASK);
            if (!Enum.IsDefined(typeof(MessageSource), messageSourceByte))
            {
                return new FailedMessage($"Received unexpected message source 0x{messageSourceByte:x2}", [.. frameBytes]);
            }
            MessageSource messageSource = (MessageSource)messageSourceByte;

            uint messageIdentifier = (byte)(unesecaptedBytes[0] & SerialCommunicationConstants.TYPE_MASK);
            messageIdentifier <<= 8;
            messageIdentifier |= unesecaptedBytes[1];
            messageIdentifier <<= 8;
            messageIdentifier |= unesecaptedBytes[2];
            messageIdentifier <<= 8;
            messageIdentifier |= unesecaptedBytes[3];

            var escapedFramePayload = UnescapeBytes(unesecaptedBytes[4..^1]);

            switch (messageType)
            {
                case MessageType.Identification:
                    {
                        return ParseIdentificationMessage(messageSource, messageIdentifier, escapedFramePayload.ToArray());
                    }
                case MessageType.SwitchStateChange:
                    {
                        return ParseSwitchStateChangeMessage(messageSource, messageIdentifier, escapedFramePayload.ToArray());
                    }
                case MessageType.AsciiLog:
                    {
                        return ParseAsciiLogMessage(messageSource, messageIdentifier, escapedFramePayload.ToArray());
                    }
                default:
                    {
                        return new FailedMessage($"Received unexpected message type code 0x{messageTypeCodeByte:x2}", [.. frameBytes]);
                    }
            }
        }

        private GenericMessage ParseSwitchStateChangeMessage(MessageSource messageSource, uint messageIdentifier, byte[] unescapedFramePayload)
        {
            var expectedLength = 1;
            if (unescapedFramePayload.Length != expectedLength)
            {
                return FailedMessage.BadPayloadLength(unescapedFramePayload.Length, MessageType.SwitchStateChange, expectedLength);
            }

            byte singleByte = unescapedFramePayload[0];
            byte switchStateByte = (byte)(singleByte & 0b10000000);
            byte switchNumberByte = (byte)(singleByte & 0b01111111);

            if (!Enum.IsDefined(typeof(SwitchState), switchStateByte))
            {
                return new FailedMessage($"Received unexpected switch state byte code 0x{switchStateByte:x2}", unescapedFramePayload);
            }

            return new SwitchStateChangeMessage(messageSource, messageIdentifier, switchNumberByte, (SwitchState) switchStateByte);
        }

        private GenericMessage ParseIdentificationMessage(MessageSource messageSource, uint messageIdentifier, byte[] unescapedFramePayload)
        {
            var expectedLength = 35;
            if (unescapedFramePayload.Length != expectedLength)
            {
                return FailedMessage.BadPayloadLength(unescapedFramePayload.Length, MessageType.Identification, expectedLength);
            }

            var productIdBytes = unescapedFramePayload.Skip(0).Take(16).ToArray();
            var productVersionBytes = unescapedFramePayload.Skip(16).Take(3).ToArray();
            var serialIdBytes = unescapedFramePayload.Skip(19).Take(16).ToArray();

            Guid productId = new Guid(productIdBytes);
            if (PadConstants.MACRO_PAD_PRODUCT_ID != productId)
            {
                return new FailedMessage($"Expected specific macro pad product ID {PadConstants.MACRO_PAD_PRODUCT_ID} but got {productId}.", unescapedFramePayload);
            }

            Version macroPadVersion = new Version(productVersionBytes[0], productVersionBytes[1], productVersionBytes[2]);

            Guid serialId = new(serialIdBytes);

            return new IdentificationMessage(messageSource, messageIdentifier, productId, macroPadVersion, serialId);
        }

        private GenericMessage ParseAsciiLogMessage(MessageSource messageSource, uint messageIdentifier, byte[] unescapedFramePayload)
        {
            var logMessage = System.Text.Encoding.UTF8.GetString(unescapedFramePayload, 0, unescapedFramePayload.Length);
            return new AsciiLogMessage(messageSource, messageIdentifier, logMessage);
        }

        public IEnumerable<byte[]> SplitBytes(byte[] bytes)
        {
            var frameBeginIndices = bytes.Select((b, i) => new { Byte = b, Index = i }).Where(t => t.Byte == SerialCommunicationConstants.FRAME_BEGIN).ToList();

            if (frameBeginIndices.Count > 0)
            {
                if (frameBeginIndices.Count == 1)
                {
                    // If there's only one frame, simply return all the bytes.
                    yield return bytes.ToArray();
                }
                else
                {
                    for (var i = 1; i < frameBeginIndices.Count; i++)
                    {
                        var startIndex = frameBeginIndices[i - 1].Index;
                        var nextIndex = frameBeginIndices[i].Index;
                        yield return bytes.Skip(startIndex).Take(nextIndex - startIndex).ToArray();
                    }
                }
            }
        }

        public void AddBytes(IList<byte> bytes)
        {
            if (null == _currentFrame)
            {
                _currentFrame = [];
            }

            foreach (var b in bytes)
            {
                switch (_parserState)
                {
                    case ParserState.WaitingForBegin:
                        {
                            if (SerialCommunicationConstants.FRAME_BEGIN == b)
                            {
                                _currentFrame = [b];
                                _parserState = ParserState.InFrame;
                            }
                            break;
                        }
                    case ParserState.InFrame:
                        {
                            _currentFrame.Add(b);
                            if (SerialCommunicationConstants.FRAME_BEGIN == b)
                            {
                                _parserState = ParserState.InFrame;
                            }
                            else if (SerialCommunicationConstants.FRAME_END == b)
                            {
                                GenericMessage message = ParseMessage(_currentFrame);
                                if (null != MessageReceivedEvent)
                                {
                                    MessageReceivedEvent(message);
                                }
                                _parserState = ParserState.WaitingForBegin;
                            }
                            break;

                        }
                }
            }
        }
    }

    public class SwitchStateChangeMessage : GenericMessage
    {
        public SwitchStateChangeMessage(MessageSource messageSource, uint messageIdentifier, byte switchNumber, SwitchState switchState) : base(messageSource, messageIdentifier)
        {
            this.SwitchNumber = switchNumber;
            this.SwitchState = switchState;
        }

        public byte SwitchNumber { get; }
        public SwitchState SwitchState { get; }
    }
}