using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ModuloFivePadClient.DesktopClient.Models;
using ModuloFivePadClient.Utilities.Extensions.ForEnumerables;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public delegate void MacroPadsModifiedHandler(MacroPadListViewModel sender);

    // Partial, since RelayCommand seems to insist upon it.
    public partial class MacroPadListViewModel : ViewModelBase
    {
        private readonly ILogger? _logger;
        public MacroPadListViewModel()
        {
            DiscoveredMacroPads = [];
            KnownMacroPads = [];
            OfflineConfiguredMacroPads = [];

            IsAnyModalOpen = false;
            IsAssignNewMacroPadModalOpen = false;
            IsNameMacroPadModalOpen = false;

            _configurationModalMacroPad = new();

            // Strictly speaking this maybe belongs in the model rather than the view-model,
            // but that's a lot of round-tripping for something where the only effect is to
            // update the UI.
            KnownMacroPads.CollectionChanged += (_, args) =>
            {
                if (null != args.OldItems)
                {
                    foreach (var o in args.OldItems)
                    {
                        if (o is KnownMacroPadViewModel oldMacroPad)
                        {
                            oldMacroPad.PropertyChanged -= ConfiguredMacroPadPropertyChanged;
                        }
                    }
                }

                if (null != args.NewItems)
                {
                    foreach (var o in args.NewItems)
                    {
                        if (o is KnownMacroPadViewModel newMacroPad)
                        {
                            newMacroPad.PropertyChanged += ConfiguredMacroPadPropertyChanged;
                        }
                    }
                }

                UpdateCanAssignNewMacroPad();
            };
        }

        public MacroPadListViewModel(ILogger logger) : this()
        {
            this._logger = logger;
        }

        private void ConfiguredMacroPadPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateCanAssignNewMacroPad();
        }

        private void UpdateCanAssignNewMacroPad()
        {
            CanAssignNewMacroPad = KnownMacroPads.Any(mp => mp.IsOnline != true);
            foreach (var oldOffline in OfflineConfiguredMacroPads.ToList())
            {
                OfflineConfiguredMacroPads.Remove(oldOffline);
            }

            foreach (var newOffline in KnownMacroPads)
            {
                OfflineConfiguredMacroPads.Add(new MacroPadViewModel(newOffline));
            }
        }

        public ObservableCollection<MacroPadViewModel> DiscoveredMacroPads { get; }
        public ObservableCollection<KnownMacroPadViewModel> KnownMacroPads { get; }
        public ObservableCollection<MacroPadViewModel> OfflineConfiguredMacroPads { get; }

        private bool? _canAssignNewMacroPad;
        public bool? CanAssignNewMacroPad
        {
            get => _canAssignNewMacroPad;
            set { if (_canAssignNewMacroPad == value) return; _canAssignNewMacroPad = value; OnPropertyChanged(nameof(CanAssignNewMacroPad)); }
        }

        private bool? _isAnyModalOpen;
        public bool? IsAnyModalOpen
        {
            get => _isAnyModalOpen;
            set { if (_isAnyModalOpen == value) return; _isAnyModalOpen = value; OnPropertyChanged(nameof(IsAnyModalOpen)); }
        }

        private bool? _isAssignNewMacroPadModalOpen;
        public bool? IsAssignNewMacroPadModalOpen
        {
            get => _isAssignNewMacroPadModalOpen;
            set { if (_isAssignNewMacroPadModalOpen == value) return; _isAssignNewMacroPadModalOpen = value; OnPropertyChanged(nameof(IsAssignNewMacroPadModalOpen)); }
        }

        private bool? _isNameMacroPadModalOpen;
        public bool? IsNameMacroPadModalOpen
        {
            get => _isNameMacroPadModalOpen;
            set { if (_isNameMacroPadModalOpen == value) return; _isNameMacroPadModalOpen = value; OnPropertyChanged(nameof(IsNameMacroPadModalOpen)); }
        }

        private KnownMacroPadViewModel _configurationModalMacroPad;
        public KnownMacroPadViewModel ConfigurationModalMacroPad
        {
            get => _configurationModalMacroPad;
            set { if (_configurationModalMacroPad == value) return; _configurationModalMacroPad = value; OnPropertyChanged(nameof(ConfigurationModalMacroPad)); }
        }


        [RelayCommand]
        internal void ConfigureKnownMacroPad(KnownMacroPadViewModel macroPad)
        {
            // We don't use the "known" nomenclature in the UI, but it means 
            // macro pads that are in the configuration but not discovered.
            IsAnyModalOpen = true;
            IsNameMacroPadModalOpen = true;
            ConfigurationModalMacroPad = new KnownMacroPadViewModel(macroPad);
        }

        [RelayCommand]
        internal void ConfigureDiscoveredMacroPad(MacroPadViewModel macroPad)
        {
            if (macroPad is KnownMacroPadViewModel macroPadWithStatus)
            {
                // ConfigurationModalMacroPad = new KnownMacroPad(macroPadWithStatus);
                throw new InvalidOperationException("creating a configured new macro pad???");
            }
            else
            {
                ConfigurationModalMacroPad = new KnownMacroPadViewModel(macroPad) { IsConfigured = false, IsOnline = true };
            }
            IsAnyModalOpen = true;
            IsNameMacroPadModalOpen = true;
        }

        [RelayCommand]
        internal void AssignMacroPad(MacroPadViewModel newMacroPad)
        {
            IsAnyModalOpen = true;
            IsAssignNewMacroPadModalOpen = true;
        }

        [RelayCommand]
        internal void HandleConfigOkayCancel(bool okay)
        {
            if (okay)
            {
                if (ConfigurationModalMacroPad.IsConfigured == false)
                {
                    if (ConfigurationModalMacroPad.IsOnline == true)
                    {
                        _logger?.LogWarning(
                            "In {class}.{function}, {prop1} is {value} but {prop2} is {value}. This is invalid.",
                            nameof(MacroPadListViewModel),
                            nameof(HandleConfigOkayCancel),
                            nameof(ConfigurationModalMacroPad.IsConfigured),
                            ConfigurationModalMacroPad.IsConfigured,
                            nameof(ConfigurationModalMacroPad.IsOnline),
                            ConfigurationModalMacroPad.IsOnline
                        );
                    }

                    var newMacroPadToRemove = DiscoveredMacroPads.FirstOrDefault(mp => mp.SerialId == ConfigurationModalMacroPad.SerialId);

                    if (newMacroPadToRemove != null)
                    {
                        DiscoveredMacroPads.Remove(newMacroPadToRemove);
                    }

                    var configuredMacroPad = new KnownMacroPadViewModel(ConfigurationModalMacroPad)
                    {
                        IsConfigured = true,
                        IsOnline = true,
                    };
                    KnownMacroPads.Add(configuredMacroPad);
                }
                else
                {
                    var existingKnownMacroPads = KnownMacroPads.Where(mp => mp.SerialId == ConfigurationModalMacroPad.SerialId).ToList();
                    var knownMacroPad = existingKnownMacroPads.CheckAndLogTwoIsTooMany(nameof(existingKnownMacroPads), _logger);

                    if (null == knownMacroPad)
                    {
                        knownMacroPad = new KnownMacroPadViewModel(ConfigurationModalMacroPad)
                        {
                            IsConfigured = true,
                            IsOnline = true,
                        };
                        KnownMacroPads.Add(knownMacroPad);
                    }
                    else
                    {
                        knownMacroPad.Nickname = ConfigurationModalMacroPad.Nickname;
                        knownMacroPad.Switches.ReplaceAll(ConfigurationModalMacroPad.Switches.Select(sw => new SwitchViewModel(sw)));
                        knownMacroPad.IsConfigured = true;
                        knownMacroPad.IsOnline = true;
                    }
                }
                
                MacroPadsModified?.Invoke(this);
            }

            IsAnyModalOpen = false;
            IsNameMacroPadModalOpen = false;
            IsAssignNewMacroPadModalOpen = false;

        }

        [RelayCommand]
        internal void RescanMacroPads()
        {
            this.MacroPadRescanRequested?.Invoke();
        }

        public event MacroPadsModifiedHandler? MacroPadsModified;
        public event Action? MacroPadRescanRequested;
    }
}
