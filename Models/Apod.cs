﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Copernicus_Weather.Models
{
    public class Apod
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Explanation { get; set; }
        public string Copyright { get; set; }
        [NotMapped] public string Media_Type { get; set; }
        [NotMapped] public string Service_Version { get; set; }
        public string Url { get; set; }
        public string HdUrl { get; set; }
        public string LocalUrl { get; set; }
        [DataType(DataType.Date)] public DateTime Date { get; set; }
    }
}