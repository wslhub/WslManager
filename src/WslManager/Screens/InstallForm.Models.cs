using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Models
    partial class InstallForm
    {
        public override DistroInstallModel CreateDefaultViewModel()
        {
            return new DistroInstallModel()
            {
                NewName = NameGenerator.Value.GetRandomName(),
            };
        }
    }
}
