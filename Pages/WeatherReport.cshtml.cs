using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Copernicus_Weather.Models.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Copernicus_Weather.Pages
{
	public class WeatherReportModel : PageModel
	{
		public WeatherReportModel(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		private IConfiguration Configuration { get; }

		public List<Sol> Days { get; set; }

		public async Task<PageResult> OnGetAsync()
		{
			string nasaApiKey = Configuration.GetSection(key: "ApiKeys")[key: "Nasa"];
			string nasaUrl = $"https://api.nasa.gov/insight_weather/?api_key={nasaApiKey}&feedtype=json";
			Days = new List<Sol>();

			IDictionary<string, dynamic> response =
				await new HttpClient().GetFromJsonAsync<IDictionary<string, dynamic>>(requestUri: nasaUrl);

			foreach (KeyValuePair<string, dynamic> day in response)
			{
				if ((Sol)day != null)
					Days.Add(item: day);
			}

			return Page();
		}
	}
}