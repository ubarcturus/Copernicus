using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace Copernicus_Weather
{
	public class Program
	{
		private static readonly UserManager<IdentityUser> userManager;

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
			// DbInitializer.Initialize(userManager);
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
		}
	}
}