using System.Collections.Generic;
using System.Threading.Tasks;
using Copernicus_Weather.Data;
using Copernicus_Weather.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Copernicus_Weather.Pages
{
    public class ApodFavoritesModel : PageModel
    {
        private readonly Copernicus_WeatherContext _context;

        public ApodFavoritesModel(Copernicus_WeatherContext context)
        {
            _context = context;
        }

        public IList<Apod> Apod { get; set; }

        public async Task OnGetAsync()
        {
            Apod = await _context.Apod.ToListAsync();
        }
    }
}