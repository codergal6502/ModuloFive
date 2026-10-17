namespace ModuloFivePadClient.MacroPad.Serial
{
    public interface IMessageSerializerFactory
    {
        IMessageSerializer BuildMessageSerializer();
    }
}