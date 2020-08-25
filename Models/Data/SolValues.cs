#region

using System;
using System.Text.Json;

#endregion

namespace Copernicus_Weather.Models.Data
{
    public class SolValues
    {
        public double Average { get; private set; }
        private double Min { get; set; }
        private double Max { get; set; }

        public static implicit operator SolValues(JsonElement jsonElement)
        {
            try
            {
                return new SolValues
                {
                    Average = jsonElement.GetProperty("av").GetDouble(),
                    Min = jsonElement.GetProperty("mn").GetDouble(),
                    Max = jsonElement.GetProperty("mx").GetDouble()
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}