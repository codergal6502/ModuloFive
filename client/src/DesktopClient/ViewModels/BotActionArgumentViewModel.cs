namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class BotActionArgumentViewModel : ViewModelBase
    {
        public BotActionArgumentViewModel() : base() { }

        private string? _name;
        public string? Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string? _value;
        public string? Value
        {
            get => _value;
            set { if (_value == value) return; _value = value; OnPropertyChanged(nameof(Value)); }
        }

        private bool _parameterDeleted;
        public bool ParameterDeleted
        {
            get => _parameterDeleted;
            set { if (_parameterDeleted == value) return; _parameterDeleted = value; OnPropertyChanged(nameof(ParameterDeleted)); }
        }
    }
}