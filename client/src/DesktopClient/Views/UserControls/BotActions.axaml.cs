using Avalonia.Controls;
using ModuloFivePadClient.DesktopClient.Models;

namespace ModuloFivePadClient.DesktopClient.Views.UserControls;

public partial class BotActions : UserControl
{
    public BotActions()
    {
        InitializeComponent();
    }

    private void ComboBox_Initialized(object? sender, System.EventArgs e)
    {
        ControlUtilities.AutoSizeComboBox(sender);
    }
}