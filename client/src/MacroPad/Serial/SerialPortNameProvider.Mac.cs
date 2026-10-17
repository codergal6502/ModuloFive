using System.IO.Ports;

namespace ModuloFivePadClient.MacroPad.Serial
{
#if OS_MAC
    public class SerialPortNameProvider : ISerialPortNameProvider
    {
        public List<string> GetNames()
        {
            return [.. SerialPort.GetPortNames().Where(pn => pn.StartsWith("/dev/cu.usbserial"))];
        }
    }
#endif
}