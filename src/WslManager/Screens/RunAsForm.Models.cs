using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Models
    partial class RunAsForm
    {
        public override DistroRunAsRequest CreateDefaultViewModel()
        {
            return new DistroRunAsRequest();
        }
    }
}
