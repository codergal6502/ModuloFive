using ModuloFivePadClient.Configuration;

namespace ModuloFivePadClient.RunnerCore
{
    public interface IRunner
    {
        public delegate void LinkagePerformedHandler(IRunner sender, LinkagePerformedArgs args);

        void LoadConfiguration(ModuloFiveConfiguration configurationProvider);

        event LinkagePerformedHandler? LinkagePerformed;
    }
}