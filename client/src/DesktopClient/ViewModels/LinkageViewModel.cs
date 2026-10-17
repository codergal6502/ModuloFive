using System;
using System.Collections.ObjectModel;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class LinkageViewModel : ViewModelBase
    {
        public LinkageViewModel()
        {
            Arguments = [];
            _hasErrors = false;
            _botActionId = String.Empty;
        }

        private string? _macroPadName;
        public string? MacroPadName
        {
            get => _macroPadName;
            set { if (_macroPadName == value) return; _macroPadName = value; OnPropertyChanged(nameof(MacroPadName)); }
        }

        private Guid? _macroPadSerialId;
        public Guid? MacroPadSerialId
        {
            get => _macroPadSerialId;
            set { if (_macroPadSerialId == value) return; _macroPadSerialId = value; OnPropertyChanged(nameof(MacroPadSerialId)); }
        }

        private string? _switchName;
        public string? SwitchName
        {
            get => _switchName;
            set { if (_switchName == value) return; _switchName = value; OnPropertyChanged(nameof(SwitchName)); }
        }

        private int? _switchNumber;
        public int? SwitchNumberOneBased
        {
            get => _switchNumber;
            set { if (_switchNumber == value) return; _switchNumber = value; OnPropertyChanged(nameof(SwitchNumberOneBased)); }
        }

        private SwitchState? _triggerState;
        public SwitchState? TriggerState
        {
            get => _triggerState;
            set { if (_triggerState == value) return; _triggerState = value; OnPropertyChanged(nameof(TriggerState)); }
        }

        private string? _botName;
        public string? BotName
        {
            get => _botName;
            set { if (_botName == value) return; _botName = value; OnPropertyChanged(nameof(BotName)); }
        }

        private string? _botActionName;
        public string? BotActionName
        {
            get => _botActionName;
            set { if (_botActionName == value) return; _botActionName = value; OnPropertyChanged(nameof(BotActionName)); }
        }

        private string _botActionId;
        public string BotActionId
        {
            get => _botActionId;
            set { if (_botActionId == value) return; _botActionId = value; OnPropertyChanged(nameof(BotActionId)); }
        }

        private bool _actionDeleted;
        public bool ActionDeleted
        {
            get => _actionDeleted;
            set { if (_actionDeleted == value) return; _actionDeleted = value; OnPropertyChanged(nameof(ActionDeleted)); }
        }

        private bool _hasErrors;
        public bool HasErrors
        {
            get => _hasErrors;
            set { if (_hasErrors == value) return; _hasErrors = value; OnPropertyChanged(nameof(HasErrors)); }
        }

        public ObservableCollection<BotActionArgumentViewModel> Arguments { get; }
    }
}
