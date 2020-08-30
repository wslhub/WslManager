using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WslManager.Extensions;
using WslManager.Models;
using WslManager.Screens;

namespace WslManager
{
    public sealed class AppContext : ApplicationContext
    {
        static AppContext()
        {
            InitLocalDatabase();
        }

        private static WslDistroContext dbContext;
        public static BindingList<WslDistro> WslDistroList
            => dbContext.WslDistros.Local.ToBindingList();

        private static void InitLocalDatabase()
        {
            if (dbContext != null)
                return;

            var targetDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WslManager");

            if (File.Exists(targetDirectory))
                File.Delete(targetDirectory);

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var databasePath = Path.Combine(targetDirectory, "wsldistro.db");

            var options = new DbContextOptionsBuilder<WslDistroContext>()
                .UseSqlite($"Data Source={databasePath};")
                .Options;

            dbContext = new WslDistroContext(options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        public AppContext(string[] arguments)
            : base(new MainForm())
        {
            _arguments = arguments;
            _container = new Container();
            _timer = new Timer(_container)
            {
                Interval = 3000,
                Enabled = true,
            };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            RefreshDistroList();
        }

        public static void RefreshDistroList()
        {
            var table = dbContext.WslDistros;
            var distroList = WslHelpers.GetDistroList();

            foreach (var eachDistroInfo in distroList)
            {
                var distro = table.Where(x => x.DistroName == eachDistroInfo.DistroName).FirstOrDefault();

                if (distro != null)
                {
                    distro.DistroStatus = eachDistroInfo.DistroStatus;
                    distro.WSLVersion = eachDistroInfo.WSLVersion;
                    distro.IsDefault = eachDistroInfo.IsDefault;
                }
                else
                {
                    distro = new WslDistro()
                    {
                        DistroName = eachDistroInfo.DistroName,
                        DistroStatus = eachDistroInfo.DistroStatus,
                        WSLVersion = eachDistroInfo.WSLVersion,
                        IsDefault = eachDistroInfo.IsDefault,
                    };
                    table.Add(distro);
                }
            }

            dbContext.SaveChanges();
        }

        private readonly string[] _arguments;
        private Container _container;
        private Timer _timer;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container?.Dispose();
                _container = null;
            }

            base.Dispose(disposing);
        }
    }
}
