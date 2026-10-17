using Microsoft.Extensions.Logging;
using System.IO.Ports;
using ModuloFivePadClient.MacroPad.Serial.Messages;
using Microsoft.VisualBasic;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public partial class SerialMacroPadConnectionManager
    {
        private class SerialMacroPadConnection : IMacroPadConnection
        {
            public bool IdentificationReceivedAndAcknowledged { get; private set; }
            public bool ConnectionBroken { get; private set; }
            public Guid ProductId { get; private set; }
            public Version MacroPadVersion { get; private set; }
            public Guid SerialId { get; private set; }

            private readonly SerialPort _serialPort;
            private readonly IMessageProcessor _messageParser;
            private readonly IMessageSerializer _messageSerializer;
            private readonly ILogger<SerialMacroPadConnection> _logger;

            public event SwitchStateChangedEvent? SwitchStateChanged;

            public SerialMacroPadConnection(SerialPort serialPort, IMessageProcessor messageParser, IMessageSerializer messageSerializer, ILogger<SerialMacroPadConnection> logger)
            {
                this.ProductId = Guid.Empty;
                this.MacroPadVersion = new Version(0, 0, 0);
                this.SerialId = Guid.Empty;

                this._serialPort = serialPort;
                this._messageParser = messageParser;
                this._messageSerializer = messageSerializer;
                this._logger = logger;

                this.IdentificationReceivedAndAcknowledged = false;
                this.ConnectionBroken = false;

                this._serialPort.DataReceived += DataReceived;
            }

            private void DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                try
                {
                    SerialPort sp = (SerialPort)sender;
                    if (sp.BytesToRead > 0)
                    {
                        int bytesToRead;
                        while ((bytesToRead = sp.BytesToRead) > 0)
                        {
                            byte[] bytes = new byte[bytesToRead];
                            int bytesActuallyRead = sp.Read(bytes, 0, bytesToRead);

                            _messageParser.AddBytes(bytes[0..bytesActuallyRead]);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Received data but no bytes were ready to be read.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Failed to read data from the serial port.");
                    this.ConnectionBroken = true;
                }
            }

            private void MessageReceivedEventHandler(GenericMessage message)
            {
                switch (message)
                {
                    case IdentificationMessage identificationMessage:
                        {
                            AcknowledgementMessage acknowledgementMessage = new AcknowledgementMessage(identificationMessage.MessageSource, identificationMessage.MessageIdentifier);
                            byte[] bytesToWrite = _messageSerializer.SerializeMessage(acknowledgementMessage);
                            _serialPort.Write(bytesToWrite, 0, bytesToWrite.Length);
                            _logger.LogWarning("Unexpected identification after handshake; did the macro pad restart?");
                            break;
                        }
                    case SwitchStateChangeMessage switchStateMessage:
                        {
                            _logger.LogDebug($"Switch {switchStateMessage.SwitchNumber} changed to {switchStateMessage.SwitchState}.");
                            AcknowledgementMessage acknowledgementMessage = new AcknowledgementMessage(switchStateMessage.MessageSource, switchStateMessage.MessageIdentifier);
                            byte[] bytesToWrite = _messageSerializer.SerializeMessage(acknowledgementMessage);
                            _serialPort.Write(bytesToWrite, 0, bytesToWrite.Length);
                            _logger.LogDebug($"Switch State Change Acknowledgement Sent for {switchStateMessage.MessageIdentifier:x00000000}!");

                            SwitchStateChanged?.Invoke(this, switchStateMessage);

                            break;
                        }
                    case AsciiLogMessage asciiLogMessage:
                        {
                            _logger.LogInformation($"Serial Log: {asciiLogMessage.LogMessage}");

                            break;
                        }
                    case FailedMessage failedMessage:
                        {
                            _logger.LogError(failedMessage.ErrorMessage);
                            break;
                        }
                    default:
                        {
                            _logger.LogError("Unexpected message type {0}.", message.GetType());
                            break;
                        }
                }
            }



            public async Task<bool> HandshakeAsync(CancellationToken cancellationToken)
            {
                void messageReceivedForHandshake(GenericMessage messageFromPad)
                {
                    switch (messageFromPad)
                    {
                        case IdentificationMessage identificationMessage:
                            {
                                var responseMessage = new AcknowledgementMessage(identificationMessage.MessageSource, identificationMessage.MessageIdentifier);
                                byte[] bytesToWrite = _messageSerializer.SerializeMessage(responseMessage);
                                _serialPort.Write(bytesToWrite, 0, bytesToWrite.Length);
                                this.IdentificationReceivedAndAcknowledged = true;
                                this._messageParser.MessageReceivedEvent -= messageReceivedForHandshake;
                                this._messageParser.MessageReceivedEvent += MessageReceivedEventHandler;
                                this.ProductId = identificationMessage.ProductId;
                                this.MacroPadVersion = identificationMessage.MacroPadVersion;
                                this.SerialId = identificationMessage.SerialId;
                                _logger.LogInformation("Client Acknowledged Message {0}", identificationMessage.MessageIdentifier);
                                break;
                            }
                        case AsciiLogMessage asciiLogMessage:
                            {
                                _logger.LogInformation("Serial Log: {0}", asciiLogMessage.LogMessage);
                                break;
                            }
                    }
                }

                _messageParser.MessageReceivedEvent += messageReceivedForHandshake;
                try
                {
                    while (!IdentificationReceivedAndAcknowledged && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(125, cancellationToken);
                    }
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
                {
                    return false;
                }
                finally
                {
                    this._messageParser.MessageReceivedEvent -= messageReceivedForHandshake;
                }

                return IdentificationReceivedAndAcknowledged && !cancellationToken.IsCancellationRequested;
            }

            public void Dispose()
            {
                this._serialPort.Close();
                this._serialPort.Dispose();
            }
        }
    }
}