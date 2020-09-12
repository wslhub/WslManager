using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Components
    partial class DistroFindForm
    {
        private ErrorProvider errorProvider;
        private BackgroundWorker distroCatalogQueryWorker;

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            errorProvider = new ErrorProvider(this)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };
            components.Add(errorProvider);

            distroCatalogQueryWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = false,
            };
            components.Add(distroCatalogQueryWorker);

            distroCatalogQueryWorker.DoWork += UserQueryWorker_DoWork;
            distroCatalogQueryWorker.RunWorkerCompleted += UserQueryWorker_RunWorkerCompleted;
        }

        private void UserQueryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show(this, "Task has been cancelled.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1);
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show(this, "Task completed with error. " + e.Error.Message, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (e.Result == null)
            {
                MessageBox.Show(this, "Task completed with no result.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            var response = (List<RootFsModel>)e.Result;
            distroRootFsList.DataSource = response;
        }

        private void UserQueryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var request = e.Argument as DistroRootFsFindRequest;

            var targetUrl = new Uri("https://wslhub.com/WslManager/data/install-source.json", UriKind.Absolute);
            var httpRequest = (HttpWebRequest)HttpWebRequest.CreateHttp(targetUrl);

            using var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using var responseStream = httpResponse.GetResponseStream();
            using var streamReader = new StreamReader(responseStream, new UTF8Encoding(false), true);

            var content = streamReader.ReadToEnd();
            var model = (InstallSourceModel)JsonConvert.DeserializeObject(content, typeof(InstallSourceModel));

            request.RootFsCandidates = model.RootFs.Where(x => string.Equals(WslHelpers.GetArchitectureName(), x.Value.Architecture, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();
            e.Result = request.RootFsCandidates;
        }
    }
}
