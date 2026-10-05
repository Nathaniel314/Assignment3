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
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b=>b.Trip)
                .WithMany()
                .HasForeignKey(b=>b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b=>b.Rider)
                .WithMany()
                .HasForeignKey(b => b.RiderUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
