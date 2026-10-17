using System.Collections.ObjectModel;
using System.Linq;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class KnownMacroPadViewModel : MacroPadViewModel
    {
        public KnownMacroPadViewModel() : base()
        {
            this.Switches = [];
            this._nickname = string.Empty;
        }

        public KnownMacroPadViewModel(MacroPadViewModel original) : base(original)
        {
            this._nickname = string.Empty;
            this._isConfigured = false;
            this._isOnline = false;
            this.Switches = [];
        }

        public KnownMacroPadViewModel(KnownMacroPadViewModel original) : base(original)
        {
            if (null != original)
            {
                this.Switches = [.. original.Switches.Select(s => new SwitchViewModel(s))];
                this._nickname = original._nickname;
                this._isConfigured = original._isConfigured;
                this._isOnline = original._isOnline;
            }
            else
            {
                this.Switches = [];
                this._nickname = string.Empty;
            }
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            internal set { if (_nickname == value) return; _nickname = value; OnPropertyChanged(nameof(Nickname)); }
        }

        private bool _isConfigured;
        public bool IsConfigured
        {
            get => _isConfigured;
            set { if (_isConfigured == value) return; _isConfigured = value; OnPropertyChanged(nameof(IsConfigured)); }
        }

        private bool _isOnline;
        public bool IsOnline
        {
            get => _isOnline;
            set { if (_isOnline == value) return; _isOnline = value; OnPropertyChanged(nameof(IsOnline)); }
        }

        public ObservableCollection<SwitchViewModel> Switches { get; }
    }
}
