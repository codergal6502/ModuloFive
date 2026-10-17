namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class LinkageSwitchChoice : ViewModelBase
    {
        public LinkageSwitchChoice()
        {
            _switchNumberOnceBased = 1;
            _nickname = string.Empty;
        }

        private int _switchNumberOnceBased;
        public int SwitchNumberOneBased
        {
            get => _switchNumberOnceBased;
            set { if (_switchNumberOnceBased == value) return; _switchNumberOnceBased = value; OnPropertyChanged(nameof(SwitchNumberOneBased)); }
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set { if (_nickname == value) return; _nickname = value; OnPropertyChanged(nameof(Nickname)); }
        }
    }
}
