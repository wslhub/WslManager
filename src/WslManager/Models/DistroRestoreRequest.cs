namespace WslManager.Models
{
    public sealed class DistroRestoreRequest : DistroInfoBase
    {
        public override string DistroName { get; set; }
        public string TarFilePath { get; set; }
        public string RestoreDirPath { get; set; }
        public bool SetAsDefault { get; set; }
        public bool Succeed { get; set; }
    }
}
