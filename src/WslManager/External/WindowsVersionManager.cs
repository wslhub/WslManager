using Microsoft.Win32;
using WslManager.Models;

namespace WslManager.External
{
    class WindowsVersionManager
    {
        public int CurrentVersion;

        public WindowsVersionManager()
        {
            int releaseId = int.Parse(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "").ToString());

            CurrentVersion = releaseId;
        }
    }
}
