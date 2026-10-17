using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ModuloFivePadClient.DesktopClient.Models;

namespace ModuloFivePadClient.DesktopClient.Views.UserControls;

public class OkayCancelModalResultEventArgs(RoutedEvent routedEvent, bool oldValue, bool newValue) : RoutedEventArgs(routedEvent)
{
    public bool OldValue { get; } = oldValue;
    public bool NewValue { get; } = newValue;
}

public partial class ConfigureMacroPad : UserControl
{
    public ConfigureMacroPad()
    {
        InitializeComponent();
    }

    public static readonly RoutedEvent<OkayCancelModalResultEventArgs> OkayCancelChosenEvent = RoutedEvent.Register<ConfigureMacroPad, OkayCancelModalResultEventArgs>(nameof(OkayCancelChosen), RoutingStrategies.Direct);

    public event EventHandler<OkayCancelModalResultEventArgs> OkayCancelChosen
    {
        add => AddHandler(OkayCancelChosenEvent, value);
        remove => RemoveHandler(OkayCancelChosenEvent, value);
    }

    protected virtual void OnOkayCancelChosen(bool oldValue, bool newValue)
    {
        var args = new OkayCancelModalResultEventArgs(OkayCancelChosenEvent, oldValue, newValue);
        RaiseEvent(args);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<ConfigureMacroPad, ICommand?>(nameof(Command));
    public static readonly StyledProperty<bool> CommandParameterProperty = AvaloniaProperty.Register<ConfigureMacroPad, bool>(nameof(CommandParameter));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public bool CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private void SwitchModeComboBox_Initialized(object? sender, EventArgs e)
    {
        ControlUtilities.AutoSizeComboBox(sender);
    }
}