using System;

namespace WslManager.Models
{
    public sealed class DistroInfo : DistroInfoBase
    {
        private bool _isDefault;
        private string _distroStatus;
        private string _wslVersion;

        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                if (value != _isDefault)
                {
                    _isDefault = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string DistroStatus
        {
            get => _distroStatus;
            set
            {
                if (value != _distroStatus)
                {
                    _distroStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string WSLVersion
        {
            get => _wslVersion;
            set
            {
                if (value != _wslVersion)
                {
                    _wslVersion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override string ToString()
            => $"{(IsDefault ? "Default" : "Non-Default")}, {DistroName}, {DistroStatus}, {WSLVersion}";

        public bool IsDistroStarted()
            => string.Equals(DistroStatus, "Running", StringComparison.OrdinalIgnoreCase);
    }
}
