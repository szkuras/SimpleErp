using Microsoft.EntityFrameworkCore;
using SimpleErp.Models;

namespace SimpleErp;

public class AppDbContext : DbContext
{
    public DbSet<Truck> Trucks { get; set; }
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Truck>()
            .HasIndex(t => t.Code)
            .IsUnique();

        modelBuilder.Entity<Truck>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
