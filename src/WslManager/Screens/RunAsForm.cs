using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Class Definition
    public sealed partial class RunAsForm : CodeFirstForm<DistroRunAsRequest>
    {
        public RunAsForm() : base() { }

        public RunAsForm(DistroRunAsRequest model) : base(model) { }
    }
}
