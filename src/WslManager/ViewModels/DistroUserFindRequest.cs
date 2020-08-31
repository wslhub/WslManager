namespace WslManager.ViewModels
{
    public sealed class DistroUserFindRequest : NotifiableModel
    {
        private string _distroName;
        private string[] _userCandidates;
        private string _user;

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

        public string[] UserIdCandidates
        {
            get => _userCandidates;
            set
            {
                if (value != _userCandidates)
                {
                    _userCandidates = value;
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
    }
}
