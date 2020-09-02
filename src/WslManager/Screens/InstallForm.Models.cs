using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Models
    partial class InstallForm
    {
        public override DistroInstallRequest CreateDefaultViewModel()
        {
            return new DistroInstallRequest()
            {
                NewName = NameGenerator.Value.GetRandomName(),
            };
        }
    }
}
