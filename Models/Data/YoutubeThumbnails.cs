namespace Copernicus_Weather.Models
{
	public class YoutubeThumbnails
	{
		public YoutubeThumbnail Default { get; set; }
		public YoutubeThumbnail Medium { get; set; }
		public YoutubeThumbnail High { get; set; }
		public YoutubeThumbnail Standard { get; set; }
		public YoutubeThumbnail MaxRes { get; set; }

		// public static YoutubeThumbnail GetThumbnail(dynamic video)
		// {
		//     // object[] youtubeThumbnails = video.items[0].snippet.thumbnails;
		//     foreach (var item in video.items[0].snippet.thumbnails)
		//     {

		//     }
		// }
	}
}