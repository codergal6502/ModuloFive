using System;
using System.Collections.ObjectModel;
using System.Linq;
using ModuloFivePadClient.BotConnections;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class BotActionViewModel : ViewModelBase
    {
        public BotActionViewModel() : base()
        {
            Parameters = [];
        }

        public BotActionViewModel(BotActionViewModel original) : this()
        {
            Parameters = [.. original.Parameters.Select(a => new BotActionParameterViewModel(a))];
        }

        public BotActionViewModel(BotAction original) : this()
        {
            if (original != null)
            {
                _name = original.Name;
                _identifier = original.Identifier;
                if (original.Parameters != null)
                {
                    Parameters = [.. original.Parameters.Select(a => new BotActionParameterViewModel(a))];
                }
            }
        }

        public ObservableCollection<BotActionParameterViewModel> Parameters { get; }

        private string? _name;
        public string? Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string? _identifier;
        public string? Identifier
        {
            get => _identifier;
            set { if (_identifier == value) return; _identifier = value; OnPropertyChanged(nameof(Identifier)); }
        }
    }
}