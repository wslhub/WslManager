using System;
using System.Collections.Generic;
using System.Text;

namespace WslManager.ViewModels
{
    public sealed class InstallSourceModel
    {
        // https://wslhub.com/WslManager/data/install-source.json

        public Dictionary<string, MicrosoftStoreModel> MicrosoftStoreUrls { get; set; } = new Dictionary<string, MicrosoftStoreModel> { };

        public Dictionary<string, AppxSideloadModel> AppxSideload { get; set; } = new Dictionary<string, AppxSideloadModel> { };

        public Dictionary<string, RootFsModel> RootFs { get; set; } = new Dictionary<string, RootFsModel> { };
    }
}
