using System;
using System.ComponentModel;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens.RestoreForm
{
    public sealed partial class RestoreForm : CodeFirstForm<DistroRestoreRequest>
    {
        public RestoreForm()
            : base()
        {
        }

        public override DistroRestoreRequest CreateDefaultViewModel()
        {
            return new DistroRestoreRequest()
            {
                DistroName = NameGenerator.Value.GetRandomName(),
            };
        }

        partial void _InitializeComponents(IContainer components);

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            this._InitializeComponents(components);
        }

        partial void InitializeDialog();

        protected override void InitializeUserInterface()
        {
            base.InitializeUserInterface();

            this.InitializeDialog();
        }
    }
}
