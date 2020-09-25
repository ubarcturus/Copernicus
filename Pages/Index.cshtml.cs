using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Copernicus_Weather.Data;
using Copernicus_Weather.Models;

using Newtonsoft.Json.Linq;

namespace Copernicus_Weather.Pages
{
	public class IndexModel : PageModel
	{
		private readonly string _apodDirectoryPath = $"{Directory.GetCurrentDirectory()}/wwwroot/apod/";
		private readonly Copernicus_WeatherContext _context;
		private readonly ILogger<IndexModel> _logger;
		private readonly UserManager<IdentityUser> _userManager;
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
		public string Explanation { get; private set; }

		public async Task<PageResult> OnGetAsync(string urlDateParameter)
		{
			HttpClient = new HttpClient { BaseAddress = new Uri(uriString: Request.GetDisplayUrl()) };

			// Sucht nach einem Datenbankeintrag des heutigen Datums in der Tabelle Apod und füllt den Inhalt in das Apod Objekt.
			_getDate = GetDate(urlDateParameter: urlDateParameter);
			Apod = await _context.Apod.Include(navigationPropertyPath: apod => apod.FavoredByUsers)
				.FirstOrDefaultAsync(predicate: apod => apod.Date == _getDate);

			await SetFavoredByCurrentUserAsync();

			if (Apod != null && (await HttpClient.GetAsync(requestUri: Apod.LocalSdUrl)).IsSuccessStatusCode)
			{ PictureUrl = Apod.LocalSdUrl; }
			else
			{
				try
				{
					await GrabApodAsync();

					if (Apod.Media_Type == "video") await PictureFromVideoAsync();

					Apod.ImageFileName = GetImageFileName();
					Apod.LocalSdUrl = $"apod/{Apod.ImageFileName}";

					await _context.Apod.AddAsync(entity: Apod);
					int changes = await _context.SaveChangesAsync();
					_logger.LogInformation(message: $"{changes} Datenbankänderungen gespeichert");

					SavePictureAsync(httpClient: HttpClient);
					PictureUrl = Apod.Url;
				}
				catch (Exception exception)
				{
					_logger.LogWarning(message: exception.Message);
				}
			}

			PictureUrl = PictureUrl.Replace(oldValue: "'", newValue: "%27");
			if (Apod != null)
			{
				ApodId = Apod.Id;
				Explanation = Apod.Explanation;
			}

			_logger.LogInformation(message: "Seite wird geladen");

			return Page();
		}

		public async Task<JsonResult> OnPostAddToFavoritesAsync([FromBody] string apodId)
		{
			HttpClient = new HttpClient { BaseAddress = new Uri(uriString: Request.GetDisplayUrl()) };
			Apod = await _context.Apod.Include(navigationPropertyPath: apod => apod.FavoredByUsers)
				.FirstOrDefaultAsync(predicate: apod => apod.Id == int.Parse(apodId));

			UserApod userApod = new UserApod
			{
				IdentityUserId = await GetUserIdAsync(),
				ApodId = int.Parse(s: apodId)
			};

			if (!User.Identity.IsAuthenticated) return new JsonResult(value: 2); //You are not logged in.

			try
			{
				if (Apod.Media_Type == "image")
				{
					SaveHdPictureAsync(httpClient: HttpClient);
					AddApodLocalHdUrl();
				}

				await _context.UserApod.AddAsync(entity: userApod);
				_context.Apod.Update(entity: Apod);
				int changes = await _context.SaveChangesAsync();
				_logger.LogInformation(message: $"{changes} Datenbankänderungen gespeichert");
			}
			catch (Exception exception)
			{
				_logger.LogError(message: exception.Message);
				return new JsonResult(value: 3); //Failed to add picture to favorites.
			}

			return new JsonResult(value: 1); //Picture successful added to your favorites.
		}

		public async Task<JsonResult> OnPostShowHdPictureAsync([FromBody] string apodId)
		{
			HttpClient = new HttpClient { BaseAddress = new Uri(uriString: Request.GetDisplayUrl()) };
			Apod = await _context.Apod.FirstOrDefaultAsync(predicate: apod => apod.Id == int.Parse(apodId));
			int changes = 0;

			if (Apod.LocalHdUrl == null)
			{
				SaveHdPictureAsync(httpClient: HttpClient);
				AddApodLocalHdUrl();

				_context.Apod.Update(entity: Apod);
				changes = await _context.SaveChangesAsync();
				_logger.LogInformation(message: $"{changes} Datenbankänderungen gespeichert");
			}
			return new JsonResult(Apod.LocalHdUrl);
		}

		// Allgemeine Funktionen

		private static DateTime GetDate(string urlDateParameter)
		{
			return DateTime.TryParse(s: urlDateParameter, result: out DateTime date) ? date.Date : DateTime.Now.Date;
		}

		private async Task GrabApodAsync()
		{
			string nasaApiKey = Configuration.GetSection(key: "ApiKeys")[key: "Nasa"];
			string nasaApiUrl =
				$"https://api.nasa.gov/planetary/apod?api_key={nasaApiKey}&date={_getDate:yyyy-MM-dd}";
			// Läd das JSON-Objekt mit den Informationen über das APOD von den Nasaservern.
			Apod = await HttpClient.GetFromJsonAsync<Apod>(requestUri: nasaApiUrl);
		}

		private async Task PictureFromVideoAsync()
		{
			// Fügt den Api-Key und die APOD-Kennung zur Youtube-Url hinzu,
			// wodurch das JSON-Objekt des Videos heruntergeladen und in "video" abgelegt werden kann
			string youtubeApiKey = Configuration.GetSection(key: "ApiKeys")[key: "Youtube"];
			string youtubeApiUrl =
				$"https://www.googleapis.com/youtube/v3/videos?key={youtubeApiKey}&part=snippet";
			string fullUrl = $"{youtubeApiUrl}&id={Regex.Match(input: Apod.Url, pattern: @"[\w-]+(?=\?)").Value}";
			JObject video = JObject.Parse(
				json: await (await HttpClient.GetAsync(requestUri: fullUrl)).Content.ReadAsStringAsync());
			// Erstellt das "thumbnails" Objekt und iteriert durch das JSON-Objekt bis der "thumbnail" Key erreicht ist.
			// Dessen Inhalt wird in das "thumbnails" Objekt gefüllt.
			YoutubeThumbnails thumbnails =
				video[propertyName: "items"][key: 0][key: "snippet"][key: "thumbnails"].ToObject<YoutubeThumbnails>();

			Apod.Url = GetThumbnailUrl(thumbnails: thumbnails);
		}

		private void AddApodLocalHdUrl()
		{
			Apod.LocalHdUrl = $"apod/hd_images/{Apod.ImageFileName}";
		}

		private string GetImageFileName()
		{
			string imageFileName = "";

			try
			{
				imageFileName = $"{Apod.Date:yyyy-MM-dd}_{Regex.Replace(input: Apod.Title, pattern: @"[\/:*?""<>\s]", replacement: "-")}.jpg";
			}
			catch
			{
				_logger.LogWarning(message: "Name der Bilddatei kann nicht generiert werden");
			}

			return imageFileName;
		}

		private async void SavePictureAsync(HttpClient httpClient)
		{
			byte[] picture = await (await httpClient.GetAsync(requestUri: Apod.Url)).Content.ReadAsByteArrayAsync();
			string directoryPath = Directory.CreateDirectory(path: _apodDirectoryPath).FullName;
			string fullPath = Path.Combine(path1: directoryPath, path2: Apod.ImageFileName);
			await System.IO.File.WriteAllBytesAsync(path: fullPath, bytes: picture);

			_logger.LogInformation(message: "Bild gespeichert");
		}

		private async void SaveHdPictureAsync(HttpClient httpClient)
		{
			byte[] hdPicture = await (await httpClient.GetAsync(requestUri: Apod.HdUrl)).Content.ReadAsByteArrayAsync();
			string hdDirectoryPath = Directory.CreateDirectory(path: $"{_apodDirectoryPath}hd_images/").FullName;
			string hdFullPath = Path.Combine(path1: hdDirectoryPath, path2: Apod.ImageFileName);
			await System.IO.File.WriteAllBytesAsync(path: hdFullPath, bytes: hdPicture);

			_logger.LogInformation(message: "HD-Bild gespeichert");
		}

		private static string GetThumbnailUrl(YoutubeThumbnails thumbnails)
		{
			if (thumbnails.MaxRes != null) return thumbnails.MaxRes.Url;
			if (thumbnails.Standard != null) return thumbnails.Standard.Url;
			if (thumbnails.High != null) return thumbnails.High.Url;
			if (thumbnails.Medium != null) return thumbnails.Medium.Url;
			return thumbnails.Default.Url;
		}

		private async Task SetFavoredByCurrentUserAsync()
		{
			string userId = await GetUserIdAsync();
			FavoredByCurrentUser = Apod?.FavoredByUsers.Any(predicate: userApod => userId == userApod.IdentityUserId) == true;
		}

		private async Task<string> GetUserIdAsync()
		{
			return (await _userManager.GetUserAsync(principal: User))?.Id;
		}
	}
}