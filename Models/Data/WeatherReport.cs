#region

using System.Collections.Generic;

#endregion

namespace Copernicus_Weather.Models.Data
{
    public class WeatherReport
    {
        public IDictionary<int, Sol> Days { get; set; }
    }
}