using System;
using System.Collections.Generic;
using System.Text;

namespace WslManager.ViewModels
{
    public sealed class AppxSideloadModel : NotifiableModel
    {
        private string _architecture;
        private string _displayName;
        private string _providerName;
        private string _url;

        public string Architecture
        {
            get => _architecture;
            set
            {
                if (value != _architecture)
                {
                    _architecture = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (value != _displayName)
                {
                    _displayName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ProviderName
        {
            get => _providerName;
            set
            {
                if (value != _providerName)
                {
                    _providerName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Url
        {
            get => _url;
            set
            {
                if (value != _url)
                {
                    _url = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
