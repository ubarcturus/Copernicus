#region

using Copernicus_Weather.Areas.Identity;
using Microsoft.AspNetCore.Hosting;

#endregion

[assembly: HostingStartup(typeof(IdentityHostingStartup))]

namespace Copernicus_Weather.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => { });
        }
    }
}