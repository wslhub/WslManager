using System.ComponentModel;

namespace WslManager.Models
{
    public sealed class DistroBackupRequest : DistroInfoBase, INotifyPropertyChanged
    {
        private string _saveFilePath;
        private bool _succeed;

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
