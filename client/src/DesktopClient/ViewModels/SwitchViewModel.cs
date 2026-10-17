namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class SwitchViewModel : ViewModelBase
    {
        public SwitchViewModel()
        {
            this._switchMode = SwitchMode.Momentary;
            this._logicalState = SwitchState.Unknown;
            this._nickname = string.Empty;
            this._switchNumberOneBased = 1;
        }

        public SwitchViewModel(SwitchViewModel s) : base()
        {
            this._switchMode = s._switchMode;
            this._nickname = s._nickname;
            this._switchNumberOneBased = s._switchNumberOneBased;
            this._logicalState = s._logicalState;
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set { if (_nickname == value) return; _nickname = value; OnPropertyChanged(nameof(Nickname)); }
        }

        private int _switchNumberOneBased;
        public int SwitchNumberOneBased
        {
            get => _switchNumberOneBased;
            set { if (_switchNumberOneBased == value) return; _switchNumberOneBased = value; OnPropertyChanged(nameof(SwitchNumberOneBased)); }
        }
        private SwitchMode _switchMode;
        public SwitchMode SwitchMode
        {
            get => _switchMode;
            set { if (_switchMode == value) return; _switchMode = value; OnPropertyChanged(nameof(SwitchMode)); }
        }

        private SwitchState _physicalState;
        public SwitchState PhysicalState
        {
            get => _physicalState;
            set { if (_physicalState == value) return; _physicalState = value; OnPropertyChanged(nameof(PhysicalState)); }
        }

        private SwitchState _logicalState;
        public SwitchState LogicalState
        {
            get => _logicalState;
            set { if (_logicalState == value) return; _logicalState = value; OnPropertyChanged(nameof(LogicalState)); }
        }
    }
}
