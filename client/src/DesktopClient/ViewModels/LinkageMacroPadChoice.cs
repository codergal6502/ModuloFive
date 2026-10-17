using System;
using System.Collections.ObjectModel;
using System.Linq;
using ModuloFivePadClient.DesktopClient.Models;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class LinkageMacroPadChoice : ViewModelBase
    {
        public LinkageMacroPadChoice()
        {
            _serialId = Guid.Empty;
            _nickname = String.Empty;
            SwitchChoices = [];
        }

        public LinkageMacroPadChoice(KnownMacroPadViewModel mp) : this()
        {
            _serialId = mp.SerialId;
            _nickname = mp.Nickname ?? string.Empty;
            SwitchChoices.AddRange(
                mp.Switches.Select
                    (
                        s => new LinkageSwitchChoice
                        {
                            Nickname = s.Nickname ?? string.Empty,
                            SwitchNumberOneBased = s.SwitchNumberOneBased
                        }
                    )
            );
        }

        public ObservableCollection<LinkageSwitchChoice> SwitchChoices { get;  }

        private Guid _serialId;
        public Guid SerialId
        {
            get => _serialId;
            set { if (_serialId == value) return; _serialId = value; OnPropertyChanged(nameof(SerialId)); }
        }

        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set { if (_nickname == value) return; _nickname = value; OnPropertyChanged(nameof(Nickname)); }
        }
    }
}
