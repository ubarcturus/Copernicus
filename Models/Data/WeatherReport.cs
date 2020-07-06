using System.Collections.Generic;

namespace Copernicus_Weather.Models.Data
{
    public class WeatherReport
    {
        public IDictionary<int, Sol> Days { get; set; }
    }
}