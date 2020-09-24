using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WslManager.Models
{
    [Table("wsl_distro")]
    public class WslDistro
    {
        [Key]
        [Required(AllowEmptyStrings = false)]
        [DisplayName("Distro Name")]
        public string DistroName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayName("Distro Status")]
        public string DistroStatus { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayName("WSL Version")]
        public int WSLVersion { get; set; } = 1;

        [Required]
        [DisplayName("Is Default?")]
        public bool IsDefault { get; set; } = false;
    }
}
