namespace WslManager.ViewModels
{
    public sealed class DistroRestoreRequest : NotifiableModel
    {
        private string _newName;
        private string _tarFilePath;
        private string _restoreDirPath;
        private bool _setAsDefault;

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

        public string TarFilePath
        {
            get => _tarFilePath;
            set
            {
                if (value != _tarFilePath)
                {
                    _tarFilePath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string RestoreDirPath
        {
            get => _restoreDirPath;
            set
            {
                if (value != _restoreDirPath)
                {
                    _restoreDirPath = value;
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
