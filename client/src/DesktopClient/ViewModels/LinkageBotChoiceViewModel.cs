using System.Collections.ObjectModel;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{    
    public class LinkageBotChoiceViewModel : ViewModelBase
    {
        public LinkageBotChoiceViewModel()
        {
            _name = "";
            _nickname = "";
            ActionChoices = [];
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set { if (_nickname == value) return; _nickname = value; OnPropertyChanged(nameof(Nickname)); }
        }

        public ObservableCollection<LinkageActionChoiceViewModel> ActionChoices { get; set; }
    }
}
