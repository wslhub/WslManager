using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Class Definition
    public sealed partial class PropertiesForm : CodeFirstForm<DistroPropertyRequest>
    {
        public PropertiesForm() : base() { }

        public PropertiesForm(DistroPropertyRequest model) : base(model) { }
    }
}
