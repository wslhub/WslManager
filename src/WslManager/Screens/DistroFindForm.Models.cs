using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Models
    partial class DistroFindForm
    {
        public override DistroRootFsFindRequest CreateDefaultViewModel()
        {
            return new DistroRootFsFindRequest();
        }
    }
}
