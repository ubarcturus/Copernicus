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
        }

        public DbSet<Apod> Apod { get; set; }
    }
}