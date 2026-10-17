using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ModuloFivePadClient.DesktopClient.ViewModels;
using ModuloFivePadClient.DesktopClient.Views;
using ModuloFivePadClient.DesktopClient.Views.UserControls;

namespace ModuloFivePadClient.DesktopClient
{
    // See https://docs.avaloniaui.net/docs/data-templates/view-locator
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object? data)
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException("This is only used in design mode.");
            }
            return data switch
            {
                MainWindowViewModel => new MainWindow(),
                MacroPadListViewModel => new MacroPadList(),
                _ => new TextBlock { Text = $"No view for {data?.GetType()?.Name ?? "null"}." }
            };
        }

        public bool Match(object? data) => data is ViewModelBase;
    }
}