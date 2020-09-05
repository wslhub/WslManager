using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Class Definition
    public sealed partial class DistroFindForm : CodeFirstForm<DistroRootFsFindRequest>
    {
        public DistroFindForm() : base() { }

        public DistroFindForm(DistroRootFsFindRequest model) : base(model) { }
    }
}
