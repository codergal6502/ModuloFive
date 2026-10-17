using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ModuloFivePadClient.Configuration;
using ModuloFivePadClient.DesktopClient.Models;
using ModuloFivePadClient.DesktopClient.Utilities;
using ModuloFivePadClient.DesktopClient.Views;
using ModuloFivePadClient.Utilities;

namespace ModuloFivePadClient.DesktopClient
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var collection = new ServiceCollection();
            collection.AddCommonServices();

            var services = collection.BuildServiceProvider();

            var vm = services.GetRequiredService<MainWindowKitchenSinkModel>().MainWindowViewModel;
            var cfgProvider = services.GetRequiredService<IModuloFivePadConfigurationProvider>();

            vm.ClearAndPopulateFromConfiguration(cfgProvider.CloneCurrentConfigurationCopy());

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm,
                };

                desktop.ShutdownRequested += (_, args) =>
                {
                    cfgProvider.WriteConfiguration();
                };
            }

            base.OnFrameworkInitializationCompleted();

            // When saving the config file is less under-engineered, see https://robbelroot.de/blog/handling-the-application-exit-in-avaloniaui-based-net-apps/.
        }

        private void ApplicationMenuAbout_Click(object? sender, System.EventArgs e)
        {
            // See https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/relaycommand
            Debug.Write("about the app!");
        }
    }
}