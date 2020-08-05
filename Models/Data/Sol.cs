using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Copernicus_Weather.Models.Data
{
    public class Sol
    {
        [Display(Name = "Day of the year")] public string DayOfTheYear { get; set; }

        [Display(Name = "Earth day")] [DataType(DataType.Date)] public DateTime FirstUtc { get; set; }

        [Display(Name = "temperature")] public SolValues AtmosphericTemperature { get; set; }

        [Display(Name = "wind speed")] public SolValues HorizontalWindSpeed { get; set; }

        private SolValues Pressure { get; set; }
        public string Season { get; set; }

        public static implicit operator Sol(KeyValuePair<string, dynamic> keyValuePair)
        {
            try
            {
                var (key, value) = keyValuePair;
                return new Sol
                {
                    DayOfTheYear = key,
                    AtmosphericTemperature = value.GetProperty("AT"),
                    FirstUtc = value.GetProperty("First_UTC").GetDateTime(),
                    HorizontalWindSpeed = value.GetProperty("HWS"),
                    Pressure = value.GetProperty("PRE"),
                    Season = value.GetProperty("Season").GetString()
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}