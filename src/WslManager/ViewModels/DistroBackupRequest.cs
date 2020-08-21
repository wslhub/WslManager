namespace WslManager.ViewModels
{
    public sealed class DistroBackupRequest : NotifiableModel
    {
        private string _distroName;
        private string _saveFilePath;
        private bool _succeed;

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

        public string SaveFilePath
        {
            get => _saveFilePath;
            set
            {
                if (value != _saveFilePath)
                {
                    _saveFilePath = value;
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
