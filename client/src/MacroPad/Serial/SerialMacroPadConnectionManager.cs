using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Collections.Concurrent;

namespace ModuloFivePadClient.MacroPad.Serial
{
    public partial class SerialMacroPadConnectionManager : IMacroPadConnectionBuilder, IDisposable
    {
        private readonly ISerialPortNameProvider _serialPortNameProvider;
        private readonly IMessageProcessorFactory _messageProcessorFactory;
        private readonly IMessageSerializerFactory _messageSerializerFactory;
        private readonly ILogger<SerialMacroPadConnectionManager> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConcurrentBag<SerialMacroPadConnection> _connections;

        public SerialMacroPadConnectionManager(
            ISerialPortNameProvider serialPortNameProvider
          , IMessageProcessorFactory messageProcessorFactory
          , IMessageSerializerFactory messageSerializerFactory
          , ILogger<SerialMacroPadConnectionManager> logger
          , ILoggerFactory loggerFactory
        )
        {
            this._serialPortNameProvider = serialPortNameProvider;
            this._messageProcessorFactory = messageProcessorFactory;
            this._messageSerializerFactory = messageSerializerFactory;
            this._logger = logger;
            this._loggerFactory = loggerFactory;
            this._connections = [];
        }

        public IEnumerable<IMacroPadConnection> ActiveMacroPadConnections { get => _connections.OfType<IMacroPadConnection>(); }

        public event MacroPadConnectedHandler? MacroPadConnected;

        public async Task<IEnumerable<IMacroPadConnection>> BuildAllAsync(CancellationToken? cancellationToken)
        {
            List<Task> tasks = [];
            var allSerialPortNames = _serialPortNameProvider.GetNames();
            _logger.LogDebug("List of serial ports: {serialPortNames}", string.Join(", ", allSerialPortNames));
            foreach (var serialPortName in allSerialPortNames)
            {
#warning what happens if this gets called twice? should be easy enough to make this smarter.
                async Task taskFunction()
                {
                    _logger.LogDebug("Attempting to connect to serial port {serialPortName}.", serialPortName);
                    SerialPort serialPort = new SerialPort(serialPortName);
                    try
                    {
#warning Treat this as an expected exception:
                        /*
                        Exception has occurred: CLR/System.UnauthorizedAccessException
                        Exception thrown: 'System.UnauthorizedAccessException' in System.IO.Ports.dll: 'Access to the port '/dev/cu.usbserial-11320' is denied.'
                         Inner exceptions found, see $exception in variables window for more details.
                         Innermost exception 	 System.IO.IOException : Resource busy
                                                                         ^^^^^^^^^^^^^ ~~~~~~~~ look for this!!!
                        */

                        serialPort.Open();
                        _logger.LogDebug("Opened serial port {serialPortName}.", serialPortName);

                        IMessageProcessor messageParser = _messageProcessorFactory.BuildMessageProcessor();
                        IMessageSerializer messageSerializer = _messageSerializerFactory.BuildMessageSerializer();
                        using var innerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? CancellationToken.None);

                        var conn = new SerialMacroPadConnection(serialPort, messageParser, messageSerializer, _loggerFactory.CreateLogger<SerialMacroPadConnection>());
                        if (await conn.HandshakeAsync(innerCts.Token))
                        {
                            _logger.LogDebug("Handshake succeeded on {serialPortName}.", serialPortName);
                            _connections.Add(conn);
                            this.MacroPadConnected?.Invoke(this, conn);
                        }
                        else
                        {
                            _logger.LogDebug("Handshake failed on {serialPortName}.", serialPortName);
                            serialPort.Close();
                            serialPort.Dispose();
                        }
                    }
                    catch
                    {
                        _logger.LogDebug("Connection threw an exception: {serialPortName}.", serialPortName);
                        serialPort.Close();
                        serialPort.Dispose();
                        throw;
                    }
                }

                tasks.Add(Task.Run(taskFunction, cancellationToken ?? CancellationToken.None));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                _logger.LogDebug("In {className}.{functionName}, operation was cancelled.", nameof(SerialMacroPadConnectionManager), nameof(BuildAllAsync));
                // Nothing to be done; whatever's in the bag is in the bag.
            }
            return _connections;
        }

        public void Dispose()
        {
            foreach (var c in _connections)
            {
                c.Dispose();
            }
        }
    }
}