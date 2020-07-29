  
  
#  Codeschnipsel
  
  
---
  
  
  
  
  
- [APOD](#apod )
  - [Code](#code )
  - [Funktionen](#funktionen )
  
  
  
  
##  APOD
  
  
Ein alter Block der alle Elemente enthält, um ein APOD zu beziehen, an zu zeigen und zu speichern.
  
###  Code
  
  
```csharp
private Apod Apod { get; set; }
public string PictureUrl { get; set; }
  
    DateTime date = DateTime.TryParse(d, out date) ? date.Date : DateTime.Now.Date;
    var nasaApiKey = Configuration.GetSection("ApiKeys")["Nasa"];
    var youtubeApiKey = Configuration.GetSection("ApiKeys")["Youtube"];
    var nasaApiUrl =
        $"https://api.nasa.gov/planetary/apod?api_key={nasaApiKey}&date={date.ToString("yyyy-MM-dd")}";
    var youtubeApiUrl = $"https://www.googleapis.com/youtube/v3/videos?key={youtubeApiKey}&part=snippet";
  
    var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri(Request.GetDisplayUrl());
  
    // Sucht nach einem Datenbankeintrag des heutigen Datums in der Tabelle Apod und füllt den Inhalt in das Apod Objekt.
    Apod = await _context.Apod.FirstOrDefaultAsync(a => a.Date == date);
  
    if (Apod == null)
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
        var v = await httpClient.GetAsync(Apod.LocalUrl);
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
    _logger.LogInformation("Seite wird geladen");
  
    return Page();
```
  
###  Funktionen
  
  
```csharp
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
```
  