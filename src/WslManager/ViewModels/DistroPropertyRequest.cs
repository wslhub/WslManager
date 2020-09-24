using System.Collections.Generic;

namespace WslManager.ViewModels
{
    public sealed class DistroPropertyRequest : NotifiableModel
    {
        private string _distroName;
        private string _location;
        private long _distroSize;
        private int _distroState;

        public string DistroName
        {
            get => _distroName;
            set
            {
                if (value != _distroName)
                {
                    _distroName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if (value != _location)
                {
                    _location = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public long DistroSize
        {
            get => _distroSize;
            set
            {
                if (value != _distroSize)
                {
                    _distroSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int DistroState
        {
            get => _distroState;
            set
            {
                if (value != _distroState)
                {
                    _distroState = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
