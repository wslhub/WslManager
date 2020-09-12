using System.Collections.Generic;

namespace WslManager.ViewModels
{
    public sealed class DistroRootFsFindRequest : NotifiableModel
    {
        private List<RootFsModel> _rootFsCandidates = new List<RootFsModel>();
        private string _distroRootFsUrl;

        public List<RootFsModel> RootFsCandidates
        {
            get => _rootFsCandidates;
            set
            {
                if (value != _rootFsCandidates)
                {
                    _rootFsCandidates = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string DistroRootFsUrl
        {
            get => _distroRootFsUrl;
            set
            {
                if (value != _distroRootFsUrl)
                {
                    _distroRootFsUrl = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
