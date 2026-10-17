using ModuloFivePadClient.Configuration;

namespace ModuloFivePadClient.RunnerCore
{
    public class RunnerLinkageArgument
    {
        public RunnerLinkageArgument(ModuloFiveConfiguration.LinkageArgument argument)
        {
            if (string.IsNullOrWhiteSpace(argument.Name))
            {
                this.Invalid = true;
                this.Name = string.Empty;
            }
            else
            {
                this.Invalid = false;
                this.Name = argument.Name;
                this.Value = argument.Value;
            }
        }

        public string Name { get; set; }
        public string? Value { get; set; }
        public bool Invalid { get; }
    }
}