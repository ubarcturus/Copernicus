using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Copernicus_Weather.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Copernicus_Weather.Pages
{
    public class WeatherReportModel : PageModel
    {
        private readonly ILogger<WeatherReportModel> _logger;

        public WeatherReportModel(ILogger<WeatherReportModel> logger, IConfiguration configuration, Copernicus_WeatherContext context)
        {
            _logger = logger;
            Configuration = configuration;
            _context = context;
        }

        public IConfiguration Configuration { get; }

        public Copernicus_WeatherContext _context { get; set; }

        public void OnGetAsync()
        {
            var nasaApiKey = Configuration.GetSection("ApiKeys")["Nasa"];
            var nasaUrl = $"https://api.nasa.gov/insight_weather/?api_key={nasaApiKey}&feedtype=json";
        }
    }
}