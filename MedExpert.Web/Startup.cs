using FluentValidation;
using FluentValidation.AspNetCore;
using MedExpert.Domain;
using MedExpert.Domain.Identity;
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
using MedExpert.Web.Services;
using MedExpert.Web.ViewModels.Account;
using MedExpert.Web.ViewModels.Analysis;
using MedExpert.Web.ViewModels.Import;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

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
            
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    bld => bld
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            
            services.AddScoped<UserManager<User>>();
            services.AddScoped<IEmailSender, EmailSender>();

            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(Configuration.GetSection("Database").Get<DatabaseConfig>().ConnectionString)
            );

            services.AddIdentity<User, Role>()//options =>
                // {
                //     options.Password.RequiredLength = 1;
                //     options.Password.RequireLowercase = false;
                //     options.Password.RequireUppercase = false;
                //     options.Password.RequireNonAlphanumeric = false;
                //     options.Password.RequireDigit = false;
                // })
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();
            
            services.AddTransient<IValidator<ReferenceIntervalModel>, ReferenceIntervalModelValidator>();
            services.AddTransient<IValidator<ImportSymptomForm>, ImportSymptomFormValidator>();
            services.AddTransient<IValidator<AnalysisFormModel>, AnalysisFormModelValidator>();
            services.AddTransient<IValidator<LoginFormModel>, AccountLoginFormModelValidator>();
            services.AddTransient<IValidator<RegisterFormModel>, AccountRegisterFormModelValidator>();

            services.AddScoped<ISpecialistService, SpecialistService>();
            services.AddScoped<IReferenceIntervalService, ReferenceIntervalService>();
            services.AddScoped<IIndicatorService, IndicatorService>();
            services.AddScoped<IDeviationLevelService, DeviationLevelService>();
            services.AddScoped<ISymptomService, SymptomService>();
            services.AddScoped<IAnalysisService, AnalysisService>();
            services.AddScoped<IAnalysisIndicatorService, AnalysisIndicatorService>();
            services.AddScoped<ISymptomCategoryService, SymptomCategoryService>();
            services.AddScoped<IAnalysisSymptomService, AnalysisSymptomService>();
            services.AddScoped<IAnalysisSymptomIndicatorService, AnalysisSymptomIndicatorService>();
            services.AddScoped<ILookupService, LookupService>();

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
            
            if (env.IsDevelopment())
            {
                app.UseCors("CorsPolicy");
            }
            
            app.UseAuthentication();
            app.UseAuthorization();

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

            if (!env.IsDevelopment()){
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";
                    
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501
                    
                    // if (env.IsDevelopment())
                    // {
                    //     spa.UseAngularCliServer(npmScript: "start");
                    // }
                });
            }
        }
    }
}