namespace WslManager.ViewModels
{
    public sealed class DistroInstallModel : NotifiableModel
    {
        private string _rootFsUrl;
        private string _newName;
        private string _installDirPath;
        private bool _setAsDefault;
        private string _downloadedTarFilePath;
        private bool _downloadInProgress;
        
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

        public string DownloadedTarFilePath
        {
            get => _downloadedTarFilePath;
            set
            {
                if (value != _downloadedTarFilePath)
                {
                    _downloadedTarFilePath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool DownloadInProgress
        {
            get => _downloadInProgress;
            set
            {
                if (value != _downloadInProgress)
                {
                    _downloadInProgress = value;
                    NotifyPropertyChanged(additionalProperties: new string[] { nameof(MakeEnabled), nameof(MakeReadOnly) });
                }
            }
        }

        public bool MakeEnabled => _downloadInProgress == false;
        public bool MakeReadOnly => _downloadInProgress == true;
    }
}
