using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ModuloFivePadClient.DesktopClient.Models;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class ConfigureLinkageViewModel : ViewModelBase
    {
        public ConfigureLinkageViewModel()
        {
            MacroPadChoices = [];
            SwitchChoices = [];
            BotChoices = [];
            BotActionChoices = [];
            Arguments = [];
            InvalidArguments = [];
        }

        public ObservableCollection<LinkageMacroPadChoice> MacroPadChoices { get; }
        public ObservableCollection<LinkageSwitchChoice> SwitchChoices { get; }
        public SwitchState[] SwitchStateChoices { get => [SwitchState.On, SwitchState.Off, SwitchState.Changed]; }
        public ObservableCollection<LinkageBotChoiceViewModel> BotChoices { get; }
        public ObservableCollection<LinkageActionChoiceViewModel> BotActionChoices { get; }
        public ObservableCollection<BotActionArgumentViewModel> Arguments { get; }
        public ObservableCollection<BotActionArgumentViewModel> InvalidArguments { get; }

        private LinkageMacroPadChoice? _selectedMacroPad;
        public LinkageMacroPadChoice? SelectedMacroPad
        {
            get => _selectedMacroPad;
            set
            {
                if (_selectedMacroPad == value) return;
                _selectedMacroPad = value;
                OnPropertyChanged(nameof(SelectedMacroPad));

                if (value == null) SwitchChoices.RemoveAll();
                else SwitchChoices.ReplaceAll(value.SwitchChoices);
            }
        }

        private LinkageSwitchChoice? _selectedSwitch;
        public LinkageSwitchChoice? SelectedSwitch
        {
            get => _selectedSwitch;
            set { if (_selectedSwitch == value) return; _selectedSwitch = value; OnPropertyChanged(nameof(SelectedSwitch)); }
        }

        private SwitchState? _selectedSwitchState;
        public SwitchState? SelectedSwitchState
        {
            get => _selectedSwitchState;
            set { if (_selectedSwitchState == value) return; _selectedSwitchState = value; OnPropertyChanged(nameof(SelectedSwitchState)); }
        }

        private LinkageBotChoiceViewModel? _selectedBot;
        public LinkageBotChoiceViewModel? SelectedBot
        {
            get => _selectedBot;
            set
            {
                if (_selectedBot == value) return;
                _selectedBot = value;
                OnPropertyChanged(nameof(SelectedBot));

                if (value == null) BotActionChoices.RemoveAll();
                else BotActionChoices.ReplaceAll(value.ActionChoices);
            }
        }

        private LinkageActionChoiceViewModel? _selectedBotAction;
        public LinkageActionChoiceViewModel? SelectedBotAction
        {
            get => _selectedBotAction;
            set
            {
                if (_selectedBotAction == value) return;
                _selectedBotAction = value;
                OnPropertyChanged(nameof(SelectedBotAction));

                PopulateArgumentsFromActionParameters();
            }
        }

        private void PopulateArgumentsFromActionParameters()
        {
            var argumentsToRemove = Arguments.ToList();
            foreach (var param in SelectedBotAction?.Parameters ?? [])
            {
                var argumentToKeep = argumentsToRemove.FirstOrDefault(a => a.Name == param.Name);
                if (argumentToKeep != null)
                {
                    argumentsToRemove.Remove(argumentToKeep);
                }
                else
                {
                    Arguments.Add(new BotActionArgumentViewModel
                    {
                        Name = param.Name,
                        ParameterDeleted = false,
                        Value = string.Empty
                    });
                }
            }
            Arguments.RemoveRange(argumentsToRemove);
        }
    }
}
