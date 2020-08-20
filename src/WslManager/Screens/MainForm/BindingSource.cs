using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
                DataSource = AppContext.DbContext.WslDistros.Local.ToBindingList(),
                AllowNew = false,
                RaiseListChangedEvents = true,
            };

            components.Add(bindingSource);
        }
    }
}
