namespace Copernicus_Weather.Models.Data
{
    public class Sol
    {
        // Atmospheric Temperature
        public SolValues At { get; set; }

        // Horizontal Wind Speed
        public SolValues Hws { get; set; }

        // Pressure
        public SolValues Pre { get; set; }
        public string Season { get; set; }
    }
}