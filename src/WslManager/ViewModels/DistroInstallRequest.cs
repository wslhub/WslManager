namespace WslManager.ViewModels
{
    public sealed class DistroInstallRequest : NotifiableModel
    {
        private string _rootFsUrl;
        private string _newName;
        private string _installDirPath;
        private bool _setAsDefault;
        
        public string RootFsUrl
        {
            get => _rootFsUrl;
            set
            {
                if (value != _rootFsUrl)
                {
                    _rootFsUrl = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string NewName
        {
            get => _newName;
            set
            {
                if (value != _newName)
                {
                    _newName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string InstallDirPath
        {
            get => _installDirPath;
            set
            {
                if (value != _installDirPath)
                {
                    _installDirPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool SetAsDefault
        {
            get => _setAsDefault;
            set
            {
                if (value != _setAsDefault)
                {
                    _setAsDefault = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
