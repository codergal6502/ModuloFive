namespace ModuloFivePadClient.BotConnections
{
    public class BotAction
    {
        public string? Identifier { get; set; }
        public string? Name { get; set; }
        public Parameter[]? Parameters { get; set; }

        public class Parameter
        {
            public string? Name { get; set; }
            public string? Type { get; set; }
        }
    }
}
