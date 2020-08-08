namespace WslManager
{
    public sealed class DistroBackupRequest
    {
        public string DistroName { get; set; }
        public string SaveFilePath { get; set; }
        public bool Succeed { get; set; }
    }
}
