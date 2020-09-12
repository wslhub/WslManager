using System;

namespace WslManager.ViewModels
{
    public sealed class AsyncDownloadContext : NotifiableModel
    {
        private Uri _url;
        private string _downloadedFilePath;

        public Uri Url
        {
            get => _url;
            set
            {
                if (value != _url)
                {
                    _url = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string DownloadedFilePath
        {
            get => _downloadedFilePath;
            set
            {
                if (value != _downloadedFilePath)
                {
                    _downloadedFilePath = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
