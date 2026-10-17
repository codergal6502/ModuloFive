using ModuloFivePadClient.BotConnections;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class LinkageParameterViewModel : ViewModelBase
    {
        public LinkageParameterViewModel()
        {
            _name = string.Empty;
        }

        public LinkageParameterViewModel(BotAction.Parameter original) : this()
        {
            _name = original.Name ?? string.Empty;
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }

    }
}
