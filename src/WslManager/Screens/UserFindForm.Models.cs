using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Models
    partial class UserFindForm
    {
        public override DistroUserFindRequest CreateDefaultViewModel()
        {
            return new DistroUserFindRequest();
        }
    }
}
