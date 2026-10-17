using Avalonia;
using ModuloFivePadClient.DesktopClient;
using System;

namespace DesktopClient
{
    sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()

                // Kinda wild that this comes in the template by default. At
                // one point I pressed F12 without thinking and to my surprise,
                // I got an ad for the Avalonia Developer Tools!
                // See https://v11.docs.avaloniaui.net/accelerate/community.
                // See https://avaloniaui.net/devtools/.
                // #if DEBUG
                //                 .WithDeveloperTools()
                // #endif
                .WithInterFont()
                .LogToTrace();
    }
}
