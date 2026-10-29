using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using ModuloFivePadClient.DesktopClient.Models;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public delegate void LinkagesModifiedHandler(LinkageListViewModel sender);

    public partial class LinkageListViewModel : ViewModelBase
    {
        public LinkageListViewModel()
        {
            Linkages = [];
            _configureLinkage = new();
            _isModalOpen = false;
        }

        public ObservableCollection<LinkageViewModel> Linkages { get; }

        private bool _isModalOpen;
        public bool IsModalOpen
        {
            get => _isModalOpen;
            set { if (_isModalOpen == value) return; _isModalOpen = value; OnPropertyChanged(nameof(IsModalOpen)); }
        }

        private bool _hasErrors;
        public bool HasErrors
        {
            get => _hasErrors;
            set { if (_hasErrors == value) return; _hasErrors = value; OnPropertyChanged(nameof(HasErrors)); }
        }

        private int? _existingLinkageIndex;
        public int? ExistingLinkageIndex
        {
            get => _existingLinkageIndex;
            set { if (_existingLinkageIndex == value) return; _existingLinkageIndex = value; OnPropertyChanged(nameof(ExistingLinkageIndex)); }
        }

        private ConfigureLinkageViewModel _configureLinkage;
        public ConfigureLinkageViewModel ConfigureLinkage
        {
            get => _configureLinkage;
            set { if (_configureLinkage == value) return; _configureLinkage = value; OnPropertyChanged(nameof(ConfigureLinkage)); }
        }

        internal void AddLinkageCommand(LinkageViewModel? lvm)
        {
            ConfigureLinkage.Arguments.RemoveAll();
            if (null == lvm)
            {
                ConfigureLinkage.SelectedBot = null;
                ConfigureLinkage.SelectedBotAction = null;
                ConfigureLinkage.SelectedMacroPad = null;
                ConfigureLinkage.SelectedSwitch = null;
                ConfigureLinkage.SelectedSwitchState = null;
                ExistingLinkageIndex = null;
            }
            else
            {
                ConfigureLinkage.Arguments.AddRange(
                    lvm.Arguments.Select(a => new BotActionArgumentViewModel { Name = a.Name, Value = a.Value, ParameterDeleted = false })
                );
                ConfigureLinkage.SelectedBot = ConfigureLinkage.BotChoices.FirstOrDefault(b => b.Name == lvm.BotName);
                ConfigureLinkage.SelectedBotAction = ConfigureLinkage.SelectedBot?.ActionChoices?.Where(a => a.BotActionId == lvm.BotActionId)?.FirstOrDefault();
                ConfigureLinkage.SelectedMacroPad = ConfigureLinkage.MacroPadChoices.FirstOrDefault(mp => mp.SerialId == lvm.MacroPadSerialId);
                ConfigureLinkage.SelectedSwitch = ConfigureLinkage.SelectedMacroPad?.SwitchChoices?.Where(s => s.SwitchNumberOneBased == lvm.SwitchNumberOneBased)?.FirstOrDefault();
                ConfigureLinkage.SelectedSwitchState = lvm.TriggerState;
                ExistingLinkageIndex = Linkages.IndexOf(lvm);
            }
            IsModalOpen = true;
        }

        [RelayCommand]
        internal void HandleConfigOkayCancel(bool okay)
        {
            if (okay)
            {
                LinkageViewModel linkageViewModel;
                if (null == ExistingLinkageIndex)
                {
                    linkageViewModel = new LinkageViewModel();
                    Linkages.Add(linkageViewModel);
                }
                else
                {
                    linkageViewModel = Linkages[(int)ExistingLinkageIndex];
                }

                linkageViewModel.MacroPadSerialId = ConfigureLinkage?.SelectedMacroPad?.SerialId;
                linkageViewModel.MacroPadName = ConfigureLinkage?.SelectedMacroPad?.Nickname;
                linkageViewModel.SwitchNumberOneBased = ConfigureLinkage?.SelectedSwitch?.SwitchNumberOneBased;
                linkageViewModel.SwitchName = ConfigureLinkage?.SelectedSwitch?.Nickname;
                linkageViewModel.TriggerState = ConfigureLinkage?.SelectedSwitchState;
#warning bot nickname isn't used anywhere
                linkageViewModel.BotName = ConfigureLinkage?.SelectedBot?.Name;
                linkageViewModel.BotActionId = ConfigureLinkage?.SelectedBotAction?.BotActionId ?? string.Empty;
                linkageViewModel.BotActionName = ConfigureLinkage?.SelectedBotAction?.Name;

                linkageViewModel.Arguments.RemoveAll();
                linkageViewModel.Arguments.AddRange(
                    ConfigureLinkage?.Arguments?.OfType<BotActionArgumentViewModel>()?.Select(ba =>
                        new BotActionArgumentViewModel
                        {
                            Name = ba.Name,
                            Value = ba.Value,
                            ParameterDeleted = false
                        }
                    ) ?? []
                );

                LinkagesModified?.Invoke(this);
            }

            IsModalOpen = false;
        }

        public event LinkagesModifiedHandler? LinkagesModified;
    }
}
