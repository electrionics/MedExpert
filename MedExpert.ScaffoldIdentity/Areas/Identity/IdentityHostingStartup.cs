using System;
using MedExpert.ScaffoldIdentity.Areas.Identity.Data;
using MedExpert.ScaffoldIdentity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(MedExpert.ScaffoldIdentity.Areas.Identity.IdentityHostingStartup))]
namespace MedExpert.ScaffoldIdentity.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<IdentityContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("IdentityContextConnection")));
                //
                // services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
                //     .AddEntityFrameworkStores<IdentityContext>();
            });
        }
    }
}