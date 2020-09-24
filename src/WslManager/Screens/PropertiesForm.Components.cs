using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Components
    partial class PropertiesForm
    {
        private ErrorProvider errorProvider;
        private BackgroundWorker propertiesCalculator;
        private BindingSource userListBindingSource;

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            errorProvider = new ErrorProvider(this)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };
            components.Add(errorProvider);

            propertiesCalculator = new BackgroundWorker();
            components.Add(propertiesCalculator);

            userListBindingSource = new BindingSource(components)
            {
                AllowNew = false,
            };

            propertiesCalculator.DoWork += PropertiesCalculator_DoWork;
            propertiesCalculator.RunWorkerCompleted += PropertiesCalculator_RunWorkerCompleted;
        }

        private void PropertiesCalculator_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this, $"Unexpected error occurred. {e.Error.Message}",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (e.Cancelled)
            {
                MessageBox.Show(this, "Task was cancelled by user request.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }
        }

        public static DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            var table = new DataTable();

            if (data == null)
                return table;

            var props = TypeDescriptor.GetProperties(typeof(T));

            for (var i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }

            var values = new object[props.Count];

            foreach (var item in data)
            {
                for (var i = 0; i < values.Length; i++)
                    values[i] = props[i].GetValue(item);
                
                table.Rows.Add(values);
            }

            return table;
        }

        private void PropertiesCalculator_DoWork(object sender, DoWorkEventArgs e)
        {
            var model = e.Argument as DistroPropertyRequest;

            if (model == null)
                throw new ArgumentException("Cannot obtain internal model reference.");

            var distroName = model.DistroName;

            if (string.IsNullOrWhiteSpace(distroName))
                return;

            var distroGuid = WslHelpers.GetDistroGuid(distroName);

            if (!distroGuid.HasValue)
                return;

            var distroLocation = WslHelpers.GetDistroLocation(distroGuid.Value);
            Invoke(new Action(() => model.Location = distroLocation));

            var totalSize = 0L;
            Invoke(new Action(() => model.DistroSize = totalSize));

            if (distroLocation != null)
            {
                var directoryInfo = new DirectoryInfo(distroLocation);
                foreach (var eachFileInfo in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    totalSize += eachFileInfo.Length;
                    Invoke(new Action(() => model.DistroSize = (long)(totalSize / 1024L)));
                }

                Invoke(new Action(() => model.DistroSize = totalSize));
            }

            var table = ToDataTable(WslHelpers.GetLinuxUserInfo(distroName));

            Invoke(new Action(() =>
            {
                userListBindingSource.DataSource = table;
            }));
        }
    }
}
