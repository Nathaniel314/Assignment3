using Microsoft.EntityFrameworkCore;
using Rider_and_Driver.Models;

namespace Rider_and_Driver.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 
        }

        public DbSet<User> Users { get; set; }

    }
}
