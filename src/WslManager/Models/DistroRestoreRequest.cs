namespace WslManager.Models
{
    public sealed class DistroRestoreRequest : DistroInfoBase
    {
        private string _tarFilePath;
        private string _restoreDirPath;
        private bool _setAsDefault;
        private bool _succeed;

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

        public bool Succeed
        {
            get => _succeed;
            set
            {
                if (value != _succeed)
                {
                    _succeed = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
