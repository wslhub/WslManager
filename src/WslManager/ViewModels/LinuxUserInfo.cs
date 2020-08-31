using System;

namespace WslManager.ViewModels
{
    [Serializable]
    public sealed class LinuxUserInfo
    {
        public LinuxUserInfo(string passwdExpression)
            : base()
        {
            var parts = passwdExpression.Trim().Split(new char[] { ':', }, StringSplitOptions.None);

            if (parts.Length < 7)
                throw new ArgumentException("Insufficient passwd row parts found.", nameof(passwdExpression));

            User = parts[0];
            PasswordVerification = parts[2];
            UserIdentifierNumber = int.Parse(parts[2]);
            GroupIdentifierNumber = int.Parse(parts[3]);
            Gecos = parts[4];
            HomeDirectoryPath = parts[5];
            LoginShellPath = parts[6];
        }

        public string User { get; set; }
        public string PasswordVerification { get; set; }
        public int UserIdentifierNumber { get; set; }
        public int GroupIdentifierNumber { get; set; }
        public string Gecos { get; set; }
        public string HomeDirectoryPath { get; set; }
        public string LoginShellPath { get; set; }

        public string[] GetGecosFields() =>
            Gecos?.Split(',');

        public override string ToString() =>
            string.Join(':', new string[] {
                User,
                PasswordVerification,
                UserIdentifierNumber.ToString(),
                GroupIdentifierNumber.ToString(),
                Gecos,
                HomeDirectoryPath,
                LoginShellPath
            });

        public bool IsRegularUser =>
            1000 <= UserIdentifierNumber && UserIdentifierNumber <= 60000;

        public bool IsSuperUser =>
            UserIdentifierNumber == 0 && GroupIdentifierNumber == 0;

        public bool CanTerminalLogin =>
            string.Equals(PasswordVerification, "x", StringComparison.OrdinalIgnoreCase);
    }
}
