using ModuloFivePadClient.BotConnections;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class BotActionParameterViewModel : ViewModelBase
    {
        public BotActionParameterViewModel()
        {
        }

        public BotActionParameterViewModel(BotActionParameterViewModel original)
        {
            _name = original.Name;
            _type = original.Type;
        }
        public BotActionParameterViewModel(BotAction.Parameter original)
        {
            _name = original.Name;
            _type = original.Type;
        }

        private string? _name;
        public string? Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string? _type;
        public string? Type
        {
            get => _type;
            set { if (_type == value) return; _type = value; OnPropertyChanged(nameof(Type)); }
        }
    }
}