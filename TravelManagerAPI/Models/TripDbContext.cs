namespace TravelManagerAPI.Models;

using Microsoft.EntityFrameworkCore;

public class TripDbContext : DbContext
{
    public TripDbContext(DbContextOptions<TripDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<ClientTrip> ClientTrips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdClient);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Pesel)
                .IsRequired()
                .HasMaxLength(11);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.IdTrip);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.StartDate)
                .IsRequired();
            entity.Property(e => e.EndDate)
                .IsRequired();
        });

        modelBuilder.Entity<ClientTrip>(entity =>
        {
            entity.HasKey(e => new { e.IdClient, e.IdTrip });

            entity.Property(e => e.PaymentDate)
                .IsRequired(false);

            entity.Property(e => e.RegisteredAt)
                .IsRequired();

            entity.HasOne(e => e.Client)
                .WithMany(c => c.ClientTrips)
                .HasForeignKey(e => e.IdClient)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.ClientTrips)
                .HasForeignKey(e => e.IdTrip)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
