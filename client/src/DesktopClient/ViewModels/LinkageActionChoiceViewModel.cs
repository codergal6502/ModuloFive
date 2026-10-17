using System.Collections.ObjectModel;
using ModuloFivePadClient.BotConnections;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class LinkageActionChoiceViewModel : ViewModelBase
    {
        public LinkageActionChoiceViewModel()
        {
            _name = "";
            _identifier = "";
            Parameters = [];
        }

        public LinkageActionChoiceViewModel(BotAction original) : this()
        {
            _name = original.Name ?? string.Empty;
            _identifier = original.Identifier ?? string.Empty;
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _identifier;
        public string Identifier
        {
            get => _identifier;
            set { if (_identifier == value) return; _identifier = value; OnPropertyChanged(nameof(Identifier)); }
        }

        public ObservableCollection<LinkageParameterViewModel> Parameters { get;}
}
}
