using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ModuloFivePadClient.DesktopClient.Models;
using ModuloFivePadClient.DesktopClient.ViewModels;

namespace ModuloFivePadClient.DesktopClient.Views.UserControls
{
    public partial class ConfigureLinkage : UserControl
    {
        public ConfigureLinkage()
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

        private void SwitchPadChoiceComboBox_Initialized(object? sender, EventArgs e)
        {
            ControlUtilities.AutoSizeComboBox<KnownMacroPadViewModel>(sender, mp => mp.Nickname ?? String.Empty);
        }

        private void BotChoiceComboBox_Initialized(object? sender, EventArgs e)
        {
            ControlUtilities.AutoSizeComboBox<BotViewModel>(sender, b => b.Name ?? String.Empty);
        }

        private void BotActionChoiceComboBox_Initialized(object? sender, EventArgs e)
        {
            ControlUtilities.AutoSizeComboBox<BotActionViewModel>(sender, b => b.Name ?? String.Empty);
        }
    }
}