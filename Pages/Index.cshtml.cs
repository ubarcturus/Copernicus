using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Copernicus_Weather.Data;
using Copernicus_Weather.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Copernicus_Weather.Pages
{
	public class IndexModel : PageModel
	{
		private readonly Copernicus_WeatherContext _context;
		private readonly ILogger<IndexModel> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly string ApodDirectoryPath = $"{Directory.GetCurrentDirectory()}/wwwroot/apod/";
		private DateTime _getDate;

		public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, Copernicus_WeatherContext context,
			UserManager<IdentityUser> userManager)
		{
			_logger = logger;
			_userManager = userManager;
			Configuration = configuration;
			_context = context;
		}

		private IConfiguration Configuration { get; }

		public Apod Apod { get; private set; }
		private HttpClient HttpClient { get; set; }

		public bool FavoredByCurrentUser { get; private set; }
		public string PictureUrl { get; private set; }
		public int ApodId { get; private set; }

		public async Task<PageResult> OnGetAsync(string urlDateParameter)
		{
			HttpClient = new HttpClient {BaseAddress = new Uri(Request.GetDisplayUrl())};

			// Sucht nach einem Datenbankeintrag des heutigen Datums in der Tabelle Apod und füllt den Inhalt in das Apod Objekt.
			_getDate = GetDate(urlDateParameter);
			Apod = await _context.Apod.Include(apod => apod.FavoredByUsers)
				.FirstOrDefaultAsync(apod => apod.Date == _getDate);

			string userId = await GetUserId();
			FavoredByCurrentUser =
				Apod != null && Apod.FavoredByUsers.Any(userApod => userId == userApod.IdentityUserId);

			if (Apod != null && (await HttpClient.GetAsync(Apod.LocalUrl)).IsSuccessStatusCode)
				PictureUrl = Apod.LocalUrl;
			else
				try
				{
					string nasaApiKey = Configuration.GetSection("ApiKeys")["Nasa"];
					string nasaApiUrl =
						$"https://api.nasa.gov/planetary/apod?api_key={nasaApiKey}&date={_getDate:yyyy-MM-dd}";
					// Läd das JSON-Objekt mit den Informationen über das APOD von den Nasaservern.
					Apod = await HttpClient.GetFromJsonAsync<Apod>(nasaApiUrl);

					if (Apod.Media_Type == "video")
					{
						// Fügt den Api-Key und die APOD-Kennung zur Youtube-Url hinzu,
						// wodurch das JSON-Objekt des Videos heruntergeladen und in "video" abgelegt werden kann
						string youtubeApiKey = Configuration.GetSection("ApiKeys")["Youtube"];
						string youtubeApiUrl =
							$"https://www.googleapis.com/youtube/v3/videos?key={youtubeApiKey}&part=snippet";
						string fullUrl = youtubeApiUrl + "&id=" + Regex.Match(Apod.Url, @"(?<=embed/)\w+").Value;
						JObject video = JObject.Parse(
							await (await HttpClient.GetAsync(fullUrl)).Content.ReadAsStringAsync());
						// Erstellt das "thumbnails" Objekt und iteriert durch das JSON-Objekt bis der "thumbnail" Key erreicht ist.
						// Dessen Inhalt wird in das "thumbnails" Objekt gefüllt.
						YoutubeThumbnails thumbnails =
							video["items"][0]["snippet"]["thumbnails"].ToObject<YoutubeThumbnails>();
						// Die von der "GetThumbnailUrl" Funktion zurückgegebene URL wird "Apod.Url" zugewiesen.
						Apod.Url = GetThumbnailUrl(thumbnails);
					}

					PictureUrl = Apod.Url;

					//
					Apod.ImageFileName = GetImageFileName();
					Apod.LocalUrl = "apod/" + Apod.ImageFileName;
					await _context.Apod.AddAsync(Apod);
					int changes = await _context.SaveChangesAsync();
					_logger.LogInformation(changes + " Datenbankänderungen gespeichert");
					SavePictureAsync(HttpClient);
				}
				catch (Exception exception)
				{
					_logger.LogWarning(exception.Message);
				}

			PictureUrl = PictureUrl.Replace("'", @"%27");
			ApodId = Apod.Id;
			_logger.LogInformation("Seite wird geladen");

			return Page();
		}

		public async Task<JsonResult> OnPostAddToFavoritesAsync([FromBody] string apodId)
		{
			HttpClient = new HttpClient {BaseAddress = new Uri(Request.GetDisplayUrl())};
			Apod = await _context.Apod.Include(apod => apod.FavoredByUsers)
				.FirstOrDefaultAsync(apod => apod.Date == _getDate);

			UserApod userApod = new UserApod
			{
				IdentityUserId = await GetUserId(),
				ApodId = int.Parse(apodId)
			};

			if (!User.Identity.IsAuthenticated) return new JsonResult("You are not logged in");

			int changes = 0;
			try
			{
				if (Apod.Media_Type == "image")
				{
					SaveHdPictureAsync(HttpClient);
					AddApodLocalHdUrl();
				}

				await _context.UserApod.AddAsync(userApod);
				_context.Apod.Update(Apod);

				changes = await _context.SaveChangesAsync();
				_logger.LogInformation(changes + " Datenbankänderungen gespeichert");
			}
			catch (Exception exception)
			{
				_logger.LogError(exception.Message);
				return new JsonResult("Failed to save changes");
			}

			return new JsonResult(changes);
		}

		public async Task<JsonResult> OnGetShowHdPictureAsync()
		{
			HttpClient = new HttpClient {BaseAddress = new Uri(Request.GetDisplayUrl())};
			Apod = await _context.Apod.FirstOrDefaultAsync(apod => apod.Date == _getDate);
			int changes = 0;

			if (Apod.LocalHdUrl != null) return new JsonResult(JObject.FromObject(((changes), (Apod.LocalHdUrl))));
			SaveHdPictureAsync(HttpClient);
			//changes = AddApodLocalHdUrl();
			return new JsonResult(JObject.FromObject(((changes), (Apod.LocalHdUrl))));
		}

		// Allgemeine Funktionen
		private static DateTime GetDate(string urlDateParameter)
		{
			return DateTime.TryParse(urlDateParameter, out DateTime date) ? date.Date : DateTime.Now.Date;
		}

		private void AddApodLocalHdUrl()
		{
			Apod.LocalHdUrl = "apod/hd_images/" + Apod.ImageFileName;
		}

		private string GetImageFileName()
		{
			string imageFileName = "";

			try
			{
				imageFileName = $"{Apod.Date:yyyy-MM-dd}_{Regex.Replace(Apod.Title, @"[\/:*?""<>\s]", "-")}.jpg";
			}
			catch
			{
				_logger.LogWarning("Name der Bilddatei kann nicht generiert werden");
			}

			return imageFileName;
		}

		private async void SavePictureAsync(HttpClient httpClient)
		{
			byte[] picture = await (await httpClient.GetAsync(Apod.Url)).Content.ReadAsByteArrayAsync();
			string directoryPath = Directory.CreateDirectory(ApodDirectoryPath).FullName;
			string fullPath = Path.Combine(directoryPath, Apod.ImageFileName);
			await System.IO.File.WriteAllBytesAsync(fullPath, picture);

			_logger.LogInformation("Bild gespeichert");
		}

		private async void SaveHdPictureAsync(HttpClient httpClient)
		{
			byte[] hdPicture = await (await httpClient.GetAsync(Apod.HdUrl)).Content.ReadAsByteArrayAsync();
			string hdDirectoryPath = Directory.CreateDirectory($"{ApodDirectoryPath}hd_images/").FullName;
			string hdFullPath = Path.Combine(hdDirectoryPath, Apod.ImageFileName);
			await System.IO.File.WriteAllBytesAsync(hdFullPath, hdPicture);

			_logger.LogInformation("HD-Bild gespeichert");
		}

		private static string GetThumbnailUrl(YoutubeThumbnails thumbnails)
		{
			if (thumbnails.MaxRes != null) return thumbnails.MaxRes.Url;
			if (thumbnails.Standard != null) return thumbnails.Standard.Url;
			if (thumbnails.High != null) return thumbnails.High.Url;
			if (thumbnails.Medium != null) return thumbnails.Medium.Url;
			return thumbnails.Default.Url;
		}

		private async Task<string> GetUserId()
		{
			return (await _userManager.GetUserAsync(User))?.Id;
		}
	}
}