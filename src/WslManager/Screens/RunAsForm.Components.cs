using System.ComponentModel;
using System.Windows.Forms;

namespace WslManager.Screens
{
    // Components
    partial class RunAsForm
    {
        private ErrorProvider errorProvider;

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            errorProvider = new ErrorProvider(this)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };
            components.Add(errorProvider);
        }
    }
}
