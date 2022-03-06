using FluentValidation;
using FluentValidation.AspNetCore;
using MedExpert.Domain;
using MedExpert.Excel;
using MedExpert.Web.Configuration;
using MedExpert.Web.Validators;
using MedExpert.Web.ViewModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MedExpert.Services.Implementation;
using MedExpert.Services.Interfaces;

namespace MedExpert.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });
            
            services.AddSingleton(Configuration.GetSection("Database").Get<DatabaseConfig>());

            services.AddDbContext<MedExpertDataContext>(options => 
            {
                options.UseSqlServer(Configuration.GetSection("Database").Get<DatabaseConfig>().ConnectionString);
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = _ => false;
            });
            
            services.AddMvc(setup => {
                //...mvc setup...
            }).AddFluentValidation();
            
            services.AddTransient<IValidator<ReferenceIntervalModel>, ReferenceIntervalModelValidator>();

            services.AddScoped<ISpecialistService, SpecialistService>();
            services.AddScoped<IReferenceIntervalService, ReferenceIntervalService>();
            services.AddScoped<IIndicatorService, IndicatorService>();
            services.AddScoped<IDeviationLevelService, DeviationLevelService>();
            services.AddScoped<ISymptomService, SymptomService>();

            services.AddScoped<ExcelParser, ExcelParser>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // endpoints.MapControllerRoute(
                //     name: "First",
                //     pattern: "Api/WeatherForecast/Get",
                //     defaults: new { controller = "WeatherForecast", action = "Get"});

                // endpoints.MapControllerRoute(
                //     name: "default",
                //     pattern: "Api/{controller}/{action}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}