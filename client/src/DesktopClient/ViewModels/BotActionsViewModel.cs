using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using ModuloFivePadClient.DesktopClient.Models;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public delegate void ModuloFiveBotConfigurationChangeNeededHandler(BotActionsViewModel sender);
    public partial class BotActionsViewModel : ViewModelBase
    {
        public BotActionsViewModel()
        {
            // Conceptually, the event handler might introduce a leak if
            // someone keeps a copy of Bots after this should get GC'd, but
            // that is (a) unlikely to happen and (b) unlikely to be a problem
            // if it does.

            Bots = [];
            BotNames = [];
            Bots.CollectionChanged += UpdateBotNames;
            _selectedBot = new();
            _nickname = string.Empty;

            this.Bots.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        {
                            if (null != args.OldItems)
                            {
                                foreach (var old in args.OldItems.OfType<BotViewModel>())
                                {
                                    old.PropertyChanged -= BotProperyChanged;
                                }
                            }
                            break;
                        }
                }
                switch (args.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        {
                            if (null != args.NewItems)
                            {
                                foreach (var @new in args.NewItems.OfType<BotViewModel>())
                                {
                                    @new.PropertyChanged += BotProperyChanged;
                                }
                            }
                            break;
                        }
                }
            };
        }

        private void BotProperyChanged(object? sender, PropertyChangedEventArgs args)
        {
            this.ModuloFiveConfigurationChangeNeeded?.Invoke(this);
        }

        private void UpdateBotNames(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            // See https://blog.stephencleary.com/2009/07/interpreting-notifycollectionchangedeve.html
            if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                // presumably this means to clear everything? This seems to be invoked by Clear(),
                // but that doesn't seem to fire events on the collection, so I don't user it.
                BotNames.RemoveAll();
                return;
            }

            switch (args.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    {
                        if (null != args.OldItems)
                        {
                            var start = args.OldStartingIndex < 0 ? 0 : args.OldStartingIndex;
                            for (int i = 0; i < args.OldItems.Count; i++)
                            {
                                BotNames.RemoveAt(start);
                            }
                        }
                        break;
                    }
            }
            switch (args.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    {
                        if (null != args.NewItems)
                        {
                            var start = args.NewStartingIndex < 0 ? 0 : args.NewStartingIndex;
                            foreach (var bot in args.NewItems.OfType<BotViewModel>().Reverse())
                            {
                                BotNames.Insert(args.NewStartingIndex, bot.Name ?? String.Empty);
                            }
                        }
                        break;
                    }
            }
        }

        public string? SelectedBotName
        {
            get => _selectedBot?.Name ?? String.Empty;
            set
            {
                if (_selectedBot?.Name == value) return;
                SelectedBot = Bots.FirstOrDefault(b => b.Name == value);
                OnPropertyChanged(nameof(SelectedBotName));
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
                if (null != _selectedBot)
                {
                    _selectedBot.IsEnabled = value;
                }
            }
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set
            {
                if (_nickname == value) return;
                _nickname = value;
                OnPropertyChanged(nameof(Nickname));
                if (null != _selectedBot)
                {
                    _selectedBot.Nickname = value;
                }
            }
        }

        private BotViewModel? _selectedBot;
        public BotViewModel? SelectedBot
        {
            get => _selectedBot;
            set
            {
                if (_selectedBot == value) return;
                _selectedBot = value;
                OnPropertyChanged(nameof(SelectedBot));
                SelectedBotName = _selectedBot?.Name;
                IsEnabled = _selectedBot?.IsEnabled ?? false;
                Nickname = _selectedBot?.Nickname ?? string.Empty;
            }
        }

        public ObservableCollection<BotViewModel> Bots { get; }

        public ObservableCollection<string> BotNames { get; }

        public event ModuloFiveBotConfigurationChangeNeededHandler? ModuloFiveConfigurationChangeNeeded;
    }
}