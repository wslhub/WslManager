namespace WslManager
{
    public sealed class DistroRestoreRequest
    {
        public string DistroName { get; set; }
        public string TarFilePath { get; set; }
        public string RestoreDirPath { get; set; }
        public bool SetAsDefault { get; set; }
        public bool Succeed { get; set; }
    }
}
