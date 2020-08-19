namespace WslManager.ViewModels
{
    public abstract class DistroInfoBase : NotifiableModel
    {
        private string _distroName;

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
    }
}
