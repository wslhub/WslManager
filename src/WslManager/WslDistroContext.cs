using Microsoft.EntityFrameworkCore;
using WslManager.Models;

namespace WslManager
{
    public class WslDistroContext : DbContext
    {
        public WslDistroContext(DbContextOptions options)
            : base(options)
        { }

        public DbSet<WslDistro> WslDistros { get; set; }
    }
}
