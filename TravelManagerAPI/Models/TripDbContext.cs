using Microsoft.EntityFrameworkCore;

namespace TravelManagerAPI.Models;

public class TripDbContext : DbContext
{
    public TripDbContext(DbContextOptions<TripDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<ClientTrip> ClientTrips { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<CountryTrip> CountryTrips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("trip");

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Client");
            entity.HasKey(e => e.IdClient);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(120);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Telephone).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Pesel).IsRequired().HasMaxLength(120);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.ToTable("Trip");
            entity.HasKey(e => e.IdTrip);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(220);
            entity.Property(e => e.DateFrom).IsRequired();
            entity.Property(e => e.DateTo).IsRequired();
            entity.Property(e => e.MaxPeople).IsRequired();
        });

        modelBuilder.Entity<ClientTrip>(entity =>
        {
            entity.ToTable("Client_Trip");
            entity.HasKey(e => new { e.IdClient, e.IdTrip });
            entity.Property(e => e.RegisteredAt).IsRequired();
            entity.Property(e => e.PaymentDate).IsRequired(false);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.ClientTrips)
                .HasForeignKey(e => e.IdClient)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.ClientTrips)
                .HasForeignKey(e => e.IdTrip)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Country");
            entity.HasKey(e => e.IdCountry);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
        });

        modelBuilder.Entity<CountryTrip>(entity =>
        {
            entity.ToTable("Country_Trip");
            entity.HasKey(e => new { e.IdCountry, e.IdTrip });

            entity.HasOne(e => e.Country)
                .WithMany(c => c.CountryTrips)
                .HasForeignKey(e => e.IdCountry)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Trip)
                .WithMany(t => t.CountryTrips)
                .HasForeignKey(e => e.IdTrip)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}