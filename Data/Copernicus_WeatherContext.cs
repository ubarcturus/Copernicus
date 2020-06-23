using Copernicus_Weather.Models;

namespace Copernicus_Weather.Data
{
    public class Copernicus_WeatherContext : IdentityDbContext
    {
        public Copernicus_WeatherContext(DbContextOptions<Copernicus_WeatherContext> options)
            : base(options)
        {
        }

        public DbSet<Apod> Apod { get; set; }
    }
}