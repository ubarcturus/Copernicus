using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Copernicus_Weather.Data;
using Copernicus_Weather.Models;
using Microsoft.AspNetCore.Http.Extensions;
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

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, Copernicus_WeatherContext context)
        {
            _logger = logger;
            Configuration = configuration;
            _context = context;
        }

        public IConfiguration Configuration { get; }

        public Copernicus_WeatherContext _context { get; set; }

        public Apod Apod { get; set; }
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

            Apod = await _context.Apod.FirstOrDefaultAsync(a => a.Date == date);
            if (Apod == null)
            {
                try
                {
                    Apod = await httpClient.GetFromJsonAsync<Apod>(nasaApiUrl);
                    if (Apod.Media_Type == "video")
                    {
                        var url = youtubeApiUrl + "&id=" + Regex.Match(Apod.Url, @"(?<=embed/)\w+").Value;
                        var response = await httpClient.GetAsync(url);
                        var video = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var thumbnails = video["items"][0]["snippet"]["thumbnails"].ToObject<YoutubeThumbnails>();
                        PictureUrl = GetThumbnailUrl(thumbnails);
                        Apod.Url = PictureUrl;
                    }
                    else
                    {
                        PictureUrl = Apod.Url;
                    }

                    Apod.LocalUrl = GetImageFileName();
                    _context.Add(Apod);
                    var v = await _context.SaveChangesAsync();
                    _logger.LogInformation("Datenbankänderungen gespeichert");
                    SavePicture(httpClient);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.Message);
                }
            }
            else
            {
                var v = await httpClient.GetAsync("apod/" + GetImageFileName());
                if (v.IsSuccessStatusCode)
                {
                    PictureUrl = Apod.LocalUrl;
                }
                else
                {
                    PictureUrl = Apod.Url;
                    _logger.LogWarning("Bild nicht gefunden");
                    SavePicture(httpClient);
                }
            }

            PictureUrl = PictureUrl.Replace("'", @"%27");
            ApodId = Apod.Id;
            _logger.LogInformation("Seite wird geladen");

            return Page();
        }

        public async void SavePicture(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync(Apod.Url);
            var picture = await response.Content.ReadAsByteArrayAsync();
            string directoryPath = Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/wwwroot/apod/").FullName;
            var fullPath = Path.Combine(directoryPath, GetImageFileName());
            await System.IO.File.WriteAllBytesAsync(fullPath, picture);
            _logger.LogInformation("Bild gespeichert");
        }

        public string GetImageFileName()
        {
            return $"{Apod.Date.ToString("yyyy-MM-dd")}_{Regex.Replace(Apod.Title, @"[\/:*?""<>\s]", "-")}.jpg";
        }

        public string GetThumbnailUrl(YoutubeThumbnails thumbnails)
        {
            if (thumbnails.MaxRes != null) return thumbnails.MaxRes.Url;
            if (thumbnails.Standard != null) return thumbnails.Standard.Url;
            if (thumbnails.High != null) return thumbnails.High.Url;
            if (thumbnails.Medium != null) return thumbnails.Medium.Url;
            return thumbnails.Default.Url;
        }

        public async Task<PageResult> OnPostAddToFavoritesAsync()
        {
            // var test = TempData["ApodId"];
            return Page();
        }
    }
}