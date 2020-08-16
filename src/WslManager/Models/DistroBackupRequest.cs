namespace WslManager.Models
{
    public sealed class DistroBackupRequest : DistroInfoBase
    {
        public override string DistroName { get; set; }
        public string SaveFilePath { get; set; }
        public bool Succeed { get; set; }
    }
}
