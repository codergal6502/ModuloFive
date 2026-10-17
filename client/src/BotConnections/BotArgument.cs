namespace ModuloFivePadClient.BotConnections
{
    public class BotArgument
    {
        public BotArgument()
        {
            Name = string.Empty;
            Value = null;
        }

        public BotArgument(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string? Value { get; }
    }
}
