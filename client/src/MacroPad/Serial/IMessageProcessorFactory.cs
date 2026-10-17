namespace ModuloFivePadClient.MacroPad.Serial
{
    public interface IMessageProcessorFactory
    {
        IMessageProcessor BuildMessageProcessor();
    }
}