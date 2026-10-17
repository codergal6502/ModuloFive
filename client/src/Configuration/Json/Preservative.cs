using System.Diagnostics.CodeAnalysis;

namespace ModuloFivePadClient.Configuration.Json
{
    public static class Preservative
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ModuloFiveConfiguration))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ModuloFiveConfiguration.Bot))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ModuloFiveConfiguration.BotAction))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ModuloFiveConfiguration.BotActionParameter))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ModuloFiveConfiguration.Linkage))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ModuloFiveConfiguration.MacroPad))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(ModuloFiveConfiguration.Switch))]
        public static void CreateStuff()
        {
            _ = Activator.CreateInstance<ModuloFiveConfiguration>();
            _ = Activator.CreateInstance<ModuloFiveConfiguration.Bot>();
            _ = Activator.CreateInstance<ModuloFiveConfiguration.BotAction>();
            _ = Activator.CreateInstance<ModuloFiveConfiguration.BotActionParameter>();
            _ = Activator.CreateInstance<ModuloFiveConfiguration.Linkage>();
            _ = Activator.CreateInstance<ModuloFiveConfiguration.MacroPad>();
            _ = Activator.CreateInstance<ModuloFiveConfiguration.Switch>();
        }
    }
}