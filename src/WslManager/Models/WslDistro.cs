using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WslManager.Models
{
    [Table("wsl_distro")]
    public class WslDistro
    {
        [Key]
        [Required(AllowEmptyStrings = false)]
        public string DistroName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string DistroStatus { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string WSLVersion { get; set; } = "1";

        [Required]
        public bool IsDefault { get; set; } = false;
    }
}
