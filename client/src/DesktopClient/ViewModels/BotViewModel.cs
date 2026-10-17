using System.Collections.ObjectModel;
using System.Linq;
using ModuloFivePadClient.BotConnections;
using ModuloFivePadClient.DesktopClient.Models;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class BotViewModel : ViewModelBase
    {
        public BotViewModel()
        {
            BotActions = [];
            _nickname = string.Empty;
        }

        public BotViewModel(BotViewModel original) : this()
        {
            if (original != null)
            {
                _name = original._name;
                BotActions = [.. original.BotActions.Select(ba => new BotActionViewModel(ba))];
            }
        }

        public BotViewModel(IBotConnection original) : this()
        {
            if (original != null)
            {
                _name = original.BotName;
                if (original.BotActions != null)
                {
                    BotActions = [.. original.BotActions.Select(ba => new BotActionViewModel(ba))];
                }
            }
        }

        public ObservableCollection<BotActionViewModel> BotActions { get; }

        private string? _name;
        public string? Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set { if (_isEnabled == value) return; _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set { if (_nickname == value) return; _nickname = value; OnPropertyChanged(nameof(Nickname)); }
        }
    }
}