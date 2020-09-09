using Copernicus_Weather.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Copernicus_Weather.Data
{
    public class Copernicus_WeatherContext : IdentityDbContext
    {
        public Copernicus_WeatherContext(DbContextOptions<Copernicus_WeatherContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Apod> Apod { get; set; }
        public DbSet<UserApod> UserApod { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Festlegen, dass die Tabelle einen Eindeutigen Schlüssel aus zwei Spalten hat
            //Erforderlich für pure join tables
            modelBuilder.Entity<UserApod>()
                .HasKey(userApod => new {userApod.IdentityUserId, userApod.ApodId});
        }
    }
}