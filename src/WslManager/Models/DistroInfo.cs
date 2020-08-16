using System;

namespace WslManager.Models
{
    public sealed class DistroInfo : DistroInfoBase
    {
        public bool IsDefault { get; set; }
        public override string DistroName { get; set; }
        public string DistroStatus { get; set; }
        public string WSLVersion { get; set; }

        public override string ToString()
            => $"{(IsDefault ? "Default" : "Non-Default")}, {DistroName}, {DistroStatus}, {WSLVersion}";

        public bool IsDistroStarted()
            => string.Equals(DistroStatus, "Running", StringComparison.OrdinalIgnoreCase);
    }
}
