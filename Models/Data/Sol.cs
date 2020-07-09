using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Copernicus_Weather.Models.Data
{
    public class Sol
    {
        [Display(Name = "Day of the year")] public string DayOfTheYear { get; set; }

        [Display(Name = "temperature")] public SolValues AtmosphericTemperature { get; set; }

        [Display(Name = "wind speed")] public SolValues HorizontalWindSpeed { get; set; }

        public SolValues Pressure { get; set; }
        public string Season { get; set; }

        public static implicit operator Sol(KeyValuePair<string, dynamic> v)
        {
            try
            {
                return new Sol
                {
                    DayOfTheYear = v.Key,
                    AtmosphericTemperature = v.Value.GetProperty("AT"),
                    HorizontalWindSpeed = v.Value.GetProperty("HWS"),
                    Pressure = v.Value.GetProperty("PRE"),
                    Season = v.Value.GetProperty("Season").GetString()
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}