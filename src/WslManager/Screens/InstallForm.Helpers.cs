using System;
using WslManager.Extensions;

namespace WslManager.Screens
{
    // Helpers
    partial class InstallForm
    {
        private static readonly Lazy<NamesGenerator> NameGenerator =
            new Lazy<NamesGenerator>(false);
    }
}
