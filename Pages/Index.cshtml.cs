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
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, Copernicus_WeatherContext context,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
            Configuration = configuration;
            _context = context;
        }

        private IConfiguration Configuration { get; }
        private Copernicus_WeatherContext _context { get; set; }

        private Apod Apod { get; set; }
        public bool FavoritedByCurrentUser { get; set; }
        public string PictureUrl { get; set; }

        [TempData] public int ApodId { get; set; }

        public async Task<PageResult> OnGetAsync(string d)
        {
            DateTime date = DateTime.TryParse(d, out date) ? date.Date : DateTime.Now.Date;
            var nasaApiKey = Configuration.GetSection("ApiKeys")["Nasa"];
            var youtubeApiKey = Configuration.GetSection("ApiKeys")["Youtube"];
            var nasaApiUrl =
                $"https://api.nasa.gov/planetary/apod?api_key={nasaApiKey}&date={date.ToString("yyyy-MM-dd")}";
            var youtubeApiUrl = $"https://www.googleapis.com/youtube/v3/videos?key={youtubeApiKey}&part=snippet";

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Request.GetDisplayUrl());

            // Sucht nach einem Datenbankeintrag des heutigen Datums in der Tabelle Apod und füllt den Inhalt in das Apod Objekt.
            Apod = await _context.Apod.Include(t => t.FavoritedByUsers).FirstOrDefaultAsync(a => a.Date == date);
            string userId = await GetUserId();
            FavoritedByCurrentUser = Apod.FavoritedByUsers.Where(i => userId == i.IdentityUserId).Any();

            if (Apod != null && (await httpClient.GetAsync(Apod.LocalUrl)).IsSuccessStatusCode) { PictureUrl = Apod.LocalUrl; }
            else
            {
                try
                {
                    // Läd das JSON-Objekt mit den Informationen über das APOD von den Nasaservern.
                    Apod = await httpClient.GetFromJsonAsync<Apod>(nasaApiUrl);

                    if (Apod.Media_Type == "video")
                    {
                        var url = youtubeApiUrl + "&id=" + Regex.Match(Apod.Url, @"(?<=embed/)\w+").Value;
                        var response = await httpClient.GetAsync(url);
                        var video = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var thumbnails = video["items"][0]["snippet"]["thumbnails"].ToObject<YoutubeThumbnails>();
                        Apod.Url = GetThumbnailUrl(thumbnails);
                    }

                    PictureUrl = Apod.Url;

                    Apod.LocalUrl = "apod/" + GetImageFileName();
                    _context.Add(Apod);
                    var v = await _context.SaveChangesAsync();
                    _logger.LogInformation(v + " Datenbankänderungen gespeichert");
                    SavePicture(httpClient);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message);
                }
            };

            PictureUrl = PictureUrl.Replace("'", @"%27");
            ApodId = Apod.Id;
            _logger.LogInformation("Seite wird geladen");

            return Page();
        }

        private string GetImageFileName()
        {
            var imageFileName = "";

            try
            {
                imageFileName =
                    $"{Apod.Date.ToString("yyyy-MM-dd")}_{Regex.Replace(Apod.Title, @"[\/:*?""<>\s]", "-")}.jpg";
            }
            catch
            {
                _logger.LogWarning("Name der Bilddatei kann nicht generiert werden");
            }

            return imageFileName;
        }

        private async void SavePicture(HttpClient httpClient)
        {
            var picture = await (await httpClient.GetAsync(Apod.Url)).Content.ReadAsByteArrayAsync();
            var directoryPath = Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/wwwroot/apod/").FullName;
            var fullPath = Path.Combine(directoryPath, GetImageFileName());
            await System.IO.File.WriteAllBytesAsync(fullPath, picture);
            _logger.LogInformation("Bild gespeichert");
        }

        private string GetThumbnailUrl(YoutubeThumbnails thumbnails)
        {
            if (thumbnails.MaxRes != null) return thumbnails.MaxRes.Url;
            if (thumbnails.Standard != null) return thumbnails.Standard.Url;
            if (thumbnails.High != null) return thumbnails.High.Url;
            if (thumbnails.Medium != null) return thumbnails.Medium.Url;
            return thumbnails.Default.Url;
        }

        public async Task<JsonResult> OnPostAddToFavoritesAsync()
        {
            var apodId = (int)TempData.Peek("ApodId");
            var UserApod = new UserApod
            {
                IdentityUserId = await GetUserId(),
                ApodId = apodId
            };

            if (!User.Identity.IsAuthenticated)
                return new JsonResult("You are not logged in");

            _context.Update(UserApod);
            var v = await _context.SaveChangesAsync();
            _logger.LogInformation(v + " Datenbankänderungen gespeichert");

            return new JsonResult(v);
        }

        private async Task<string> GetUserId()
        {
            return (await _userManager.GetUserAsync(User))?.Id;
        }
    }
}