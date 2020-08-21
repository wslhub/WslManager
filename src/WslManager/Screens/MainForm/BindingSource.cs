using System.ComponentModel;
using System.Windows.Forms;

namespace WslManager.Screens.MainForm
{
    // Binding Source
    partial class MainForm
    {
        private BindingSource bindingSource;

        partial void InitializeBindingSource(IContainer components)
        {
            bindingSource = new BindingSource()
            {
                DataSource = AppContext.WslDistroList,
                AllowNew = false,
                RaiseListChangedEvents = true,
            };
            components.Add(bindingSource);
        }
    }
}
