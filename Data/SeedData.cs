using System.Linq;
using Copernicus_Weather.Models;
using Microsoft.AspNetCore.Identity;

namespace Copernicus_Weather.Data
{
	public static class SeedData
	{
		public static void EnsureSeedData(this Copernicus_WeatherContext context, UserManager<IdentityUser> userManager)
		{
			if (!context.Users.Any())
			{
				IdentityUser user = new IdentityUser
				{
					Id = "88888888-4444-4444-4444-121212121212",
					Email = "ubarcturus@gmail.com",
					EmailConfirmed = true,
					UserName = "ubarcturus"
				};
				var v = userManager.CreateAsync(user, "Qwertz1'");
				v.Wait();
			}

			if (!context.Apod.Any())
			{
				Apod apod = new Apod
				{
					Id = 1,
					Title = "Bubblegum",
					Explanation = null,
					Copyright = null,
					Media_Type = "picture",
					Service_Version = null,
					Url = "https://apod.nasa.gov/apod/image/2008/TYC8998_ESO_960.jpg",
					HdUrl = null,
					ImageFileName = null,
					LocalUrl = null,
					LocalHdUrl = null,
					Date = default,
					FavoredByUsers = null
				};
				context.Apod.AddAsync(apod);
				context.SaveChangesAsync();
			}
		}
	}
}