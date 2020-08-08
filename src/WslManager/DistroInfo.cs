namespace WslManager
{
    public sealed class DistroInfo
    {
        public bool IsDefault { get; set; }
        public string DistroName { get; set; }
        public string DistroStatus { get; set; }
        public string WSLVersion { get; set; }

        public override string ToString()
            => $"{(IsDefault ? "Default" : "Non-Default")}, {DistroName}, {DistroStatus}, {WSLVersion}";
    }
}
