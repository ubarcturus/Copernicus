using Copernicus_Weather.Data;
using Copernicus_Weather.Models.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace Copernicus_Weather.Pages
{
    public class WeatherReportModel : PageModel
    {
        private readonly ILogger<WeatherReportModel> _logger;

        public WeatherReportModel(ILogger<WeatherReportModel> logger, IConfiguration configuration,
            Copernicus_WeatherContext context)
        {
            _logger = logger;
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public List<Sol> Days { get; set; }

        public async Task<PageResult> OnGetAsync()
        {
            string nasaApiKey = Configuration.GetSection("ApiKeys")["Nasa"];
            string nasaUrl = $"https://api.nasa.gov/insight_weather/?api_key={nasaApiKey}&feedtype=json";

            var httpClient = new HttpClient();
            var response = await httpClient.GetFromJsonAsync<IDictionary<string, dynamic>>(nasaUrl);
            Days = new List<Sol>();

            foreach (var day in response)
                if ((Sol)day != null)
                    Days.Add(day);

            return Page();
        }
    }
}