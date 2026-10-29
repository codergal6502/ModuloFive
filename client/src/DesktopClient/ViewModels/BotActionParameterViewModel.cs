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
        }
        public BotActionParameterViewModel(BotAction.Parameter original)
        {
            _name = original.Name;
        }

        private string? _name;
        public string? Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }
    }
}