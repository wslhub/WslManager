using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Models
    partial class RestoreForm
    {
        public override DistroRestoreRequest CreateDefaultViewModel()
        {
            return new DistroRestoreRequest()
            {
                NewName = NameGenerator.Value.GetRandomName(),
            };
        }
    }
}
