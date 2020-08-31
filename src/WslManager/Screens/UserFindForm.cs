using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Class Definition
    public sealed partial class UserFindForm : CodeFirstForm<DistroUserFindRequest>
    {
        public UserFindForm() : base() { }

        public UserFindForm(DistroUserFindRequest model) : base(model) { }
    }
}
