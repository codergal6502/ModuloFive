using ModuloFivePadClient.Configuration;

namespace ModuloFivePadClient.RunnerCore
{
    public class RunnerLinkageArgument
    {
        #warning FOLLOW THIS PATTERN
        public static RunnerLinkageArgument From(ModuloFiveConfiguration.LinkageArgument argument)
        {
            if (string.IsNullOrWhiteSpace(argument.Name))
            {
                return new RunnerLinkageArgument
                {
                    Invalid = true,
                    Name = string.Empty,
                };
            }
            else
            {
                return new RunnerLinkageArgument
                {
                    Invalid = false,
                    Name = argument.Name,
                    Value = argument.Value,
                };
            }
        }

        public RunnerLinkageArgument() { }

        public required string Name { get; set; }
        public string? Value { get; set; }
        public bool Invalid { get; private set; }
    }
}