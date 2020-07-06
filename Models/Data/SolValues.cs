using System.Text.Json;

namespace Copernicus_Weather.Models.Data
{
    public class SolValues
    {
        public double Average { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }

        public static implicit operator SolValues(JsonElement v)
        {
            try
            {
                return new SolValues
                {
                    Average = v.GetProperty("av").GetDouble(),
                    Min = v.GetProperty("mn").GetDouble(),
                    Max = v.GetProperty("mx").GetDouble()
                };

            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}