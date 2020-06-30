using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Copernicus_Weather.Data;
using Copernicus_Weather.Models;
using Microsoft.AspNetCore.Http.Extensions;
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

        public Apod Apod { get; set; }
        public Copernicus_WeatherContext _context { get; set; }

        public string pictureUrl { get; set; }

        public async Task<PageResult> OnGetAsync()
        {
            var nasaApiKey = Configuration.GetSection("ApiKeys")["Nasa"];
            var youtubeApiKey = Configuration.GetSection("ApiKeys")["Youtube"];
            var nasaUrl = $"https://api.nasa.gov/planetary/apod?api_key={nasaApiKey}";
            var youtubeApiUrl = $"https://www.googleapis.com/youtube/v3/videos?key={youtubeApiKey}&part=snippet";

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Request.GetDisplayUrl());
            Apod = await _context.Apod.FirstOrDefaultAsync(a => a.Date == DateTime.Now.Date);
            if (Apod == null)
            {
                try
                {
                    Apod = await httpClient.GetFromJsonAsync<Apod>(nasaUrl);
                    if (Apod.Media_Type == "video")
                    {
                        var url = youtubeApiUrl + "&id=" + Regex.Match(Apod.Url, @"(?<=embed/)\w+").Value;
                        var response = await httpClient.GetAsync(url);
                        var video = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var thumbnails = video["items"][0]["snippet"]["thumbnails"].ToObject<YoutubeThumbnails>();
                        pictureUrl = GetThumbnailUrl(thumbnails);
                        Apod.Url = pictureUrl;
                    }

                    else
                    {
                        pictureUrl = Apod.Url;
                    }

                    Apod.LocalUrl = GetImagePath();
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
                var v = await httpClient.GetAsync(GetImagePath());
                if (v.IsSuccessStatusCode)
                {
                    pictureUrl = Apod.LocalUrl;
                }
                else
                {
                    pictureUrl = Apod.Url;
                    _logger.LogInformation("Bild nicht gefunden");
                    SavePicture(httpClient);
                }
            }

            _logger.LogInformation("Seite wird geladen");
            return Page();
        }

        public async void SavePicture(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync(Apod.Url);
            var picture = await response.Content.ReadAsByteArrayAsync();
            var directoryPath = $"{Directory.GetCurrentDirectory()}/wwwroot///";
            var fullPath = Path.Combine(directoryPath, GetImagePath());
            await System.IO.File.WriteAllBytesAsync(fullPath, picture);
            _logger.LogInformation("Bild gespeichert");
        }

        public string GetImagePath()
        {
            return $"apod/{Apod.Date.ToString("yyyy-MM-dd")}_{Regex.Replace(Apod.Title, @"[\/:*?""<>\s]", "-")}.jpg";
        }

        public string GetThumbnailUrl(YoutubeThumbnails thumbnails)
        {
            if (thumbnails.MaxRes != null) return thumbnails.MaxRes.Url;
            if (thumbnails.Standard != null) return thumbnails.Standard.Url;
            if (thumbnails.High != null) return thumbnails.High.Url;
            if (thumbnails.Medium != null) return thumbnails.Medium.Url;
            return thumbnails.Default.Url;
        }
    }
}