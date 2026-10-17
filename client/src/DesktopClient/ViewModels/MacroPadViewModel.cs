using System;

namespace ModuloFivePadClient.DesktopClient.ViewModels
{
    public class MacroPadViewModel : ViewModelBase
    {
        public MacroPadViewModel()
        {
            _serialId = Guid.Empty;
        }

        public MacroPadViewModel(MacroPadViewModel original) : this()
        {
            if (null != original)
            {
                this._macroPadVersion = original._macroPadVersion;
                this._productId = original._productId;
                this._serialId = original._serialId;
            }
        }

        private Guid? _productId;
        public Guid? ProductId
        {
            get => _productId;
            set { if (_productId == value) return; _productId = value; OnPropertyChanged(nameof(ProductId)); }
        }

        private Version? _macroPadVersion;
        public Version? MacroPadVersion
        {
            get => _macroPadVersion;
            set { if (_macroPadVersion == value) return; _macroPadVersion = value; OnPropertyChanged(nameof(MacroPadVersion)); }
        }

        private Guid _serialId;
        public Guid SerialId
        {
            get => _serialId;
            set { if (_serialId == value) return; _serialId = value; OnPropertyChanged(nameof(SerialId)); }
        }

    }
}
