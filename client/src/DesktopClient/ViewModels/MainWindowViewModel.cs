using System;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(string selectedActivity = "Welcome", ILogger? logger = null)
        {
            _selectedActivity = selectedActivity;
            _logger = logger;
            _macroPadList = new();
            _botActions = new();
            _linkages = new();
        }

        private string _selectedActivity;
        private readonly ILogger? _logger;
        public const string MENU_ITEM_NAME_WELCOME = "Welcome";
        public const string MENU_ITEM_NAME_MACRO_PADS = "Macro Pads";
        public const string MENU_ITEM_NAME_BOTS_ACTIONS = "Bots & Actions";
        public const string MENU_ITEM_NAME_LINKAGES = "Linkages";
        public const string MENU_ITEM_NAME_PREFERENCES = "Preferences";

        private MacroPadListViewModel _macroPadList;
        public MacroPadListViewModel MacroPadList { get => _macroPadList; set { if (_macroPadList == value) return; _macroPadList = value; OnPropertyChanged(nameof(MacroPadList)); } }

        private BotActionsViewModel _botActions;
        public BotActionsViewModel BotActions { get => _botActions; set { if (_botActions == value) return; _botActions = value; OnPropertyChanged(nameof(BotActions)); } }

        private LinkageListViewModel _linkages;
        public LinkageListViewModel LinkageList
        {
            get => _linkages;
            set { if (_linkages == value) return; _linkages = value; OnPropertyChanged(nameof(LinkageList)); }
        }

        public string[] MenuItemNames { get => [MENU_ITEM_NAME_WELCOME, MENU_ITEM_NAME_MACRO_PADS, MENU_ITEM_NAME_BOTS_ACTIONS, MENU_ITEM_NAME_LINKAGES, MENU_ITEM_NAME_PREFERENCES]; }

        public string SelectedActivity
        {
            get => _selectedActivity;
            set { if (value == _selectedActivity) return; _selectedActivity = value; OnPropertyChanged(nameof(SelectedActivity)); }
        }
        private string? _runnerStatus;
        public string? RunnerStatus
        {
            get => _runnerStatus;
            set { if (_runnerStatus == value) return; _runnerStatus = value; OnPropertyChanged(nameof(RunnerStatus)); }
        }

        [RelayCommand]
        public void Ready()
        {
            _logger?.LogDebug("In relay command {class}.{func}.", nameof(MainWindowViewModel), nameof(Ready));
            OnReady?.Invoke();
        }

        public event Action? OnReady;
    }
}
