using System;
using Banana.Areas.Identity.Data;
using Banana.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Banana.Areas.Identity.IdentityHostingStartup))]
namespace Banana.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<BananaContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("BananaContextConnection")));

                services.AddDefaultIdentity<BananaUser>(options =>
                {
                    options.User.AllowedUserNameCharacters = null; // allow all characters
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredUniqueChars = 4;
                })
                    .AddEntityFrameworkStores<BananaContext>()
                    .AddErrorDescriber<CustomIdentityErrorDescriber>();
            });
        }
    }
}