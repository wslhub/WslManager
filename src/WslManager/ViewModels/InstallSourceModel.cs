using System;
using System.Collections.Generic;
using System.Text;

namespace WslManager.ViewModels
{
    public sealed class InstallSourceModel
    {
        // https://wslhub.com/WslManager/data/install-source.json

        public List<MicrosoftStoreModel> MicrosoftStoreUrls { get; set; }
            = new List<MicrosoftStoreModel>();

        public List<AppxSideloadModel> AppxSideload { get; set; }
            = new List<AppxSideloadModel>();

        public List<RootFsModel> RootFs { get; set; }
            = new List<RootFsModel>();
    }
}
