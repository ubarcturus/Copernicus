using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Copernicus_Weather.Models
{
    public class YoutubeThumbnails
    {
        public YoutubeThumbnail Default { get; set; }
        public YoutubeThumbnail Medium { get; set; }
        public YoutubeThumbnail High { get; set; }
        public YoutubeThumbnail Standard { get; set; }
        public YoutubeThumbnail MaxRes { get; set; }
    }
}