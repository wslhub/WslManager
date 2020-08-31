namespace WslManager.ViewModels
{
    public sealed class DistroRunAsRequest : NotifiableModel
    {
        private string[] _distroList;
        private string _distroName;
        private string _user;
        private string _execCommandLine;
        
        public string[] DistroList
        {
            get => _distroList;
            set
            {
                if (value != _distroList)
                {
                    _distroList = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public string User
        {
            get => _user;
            set
            {
                if (value != _user)
                {
                    _user = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ExecCommandLine
        {
            get => _execCommandLine;
            set
            {
                if (value != _execCommandLine)
                {
                    _execCommandLine = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
