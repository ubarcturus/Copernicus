using Copernicus_Weather.Data;
using Copernicus_Weather.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Copernicus_Weather.Pages
{
    public class ApodFavoritesModel : PageModel
    {
        private readonly Copernicus_WeatherContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApodFavoritesModel(Copernicus_WeatherContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Apod> ApodList { get; private set; }

        public async Task OnGetAsync()
        {
            string userId = (await _userManager.GetUserAsync(User))?.Id;
            ApodList = await _context.Apod
                .Include(apod => apod.FavoredByUsers)
                .Where(apod => apod.FavoredByUsers
                    .Any(userApod => userId == userApod.IdentityUserId))
                .ToListAsync();
        }
    }
}