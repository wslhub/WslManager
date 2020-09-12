namespace WslManager.ViewModels
{
    public sealed class DistroBackupRequest : NotifiableModel
    {
        private string _distroName;
        private string _saveFilePath;

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
    }
}
