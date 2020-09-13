using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.ViewModels;

namespace WslManager.Screens
{
    // Components
    partial class UserFindForm
    {
        private ErrorProvider errorProvider;
        private BackgroundWorker userQueryWorker;

        protected override void InitializeComponents(IContainer components)
        {
            base.InitializeComponents(components);

            errorProvider = new ErrorProvider(this)
            {
                BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError,
            };
            components.Add(errorProvider);

            userQueryWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = false,
            };
            components.Add(userQueryWorker);

            userQueryWorker.DoWork += UserQueryWorker_DoWork;
            userQueryWorker.RunWorkerCompleted += UserQueryWorker_RunWorkerCompleted;
        }

        private void UserQueryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            userList.DataSource = ViewModel.UserIdCandidates;
        }

        private void UserQueryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var request = e.Argument as DistroUserFindRequest;

            if (request != null)
                request.UserIdCandidates = WslHelpers.GetLinuxUserInfo(request.DistroName)
                    .Where(x => x.IsRegularUser || x.IsSuperUser).Select(x => x.User).ToArray();
        }
    }
}
