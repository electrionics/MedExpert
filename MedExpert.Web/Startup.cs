using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using FluentValidation;
using FluentValidation.AspNetCore;
using Z.EntityFramework.Extensions;

using MedExpert.Domain;
using MedExpert.Domain.Identity;
using MedExpert.Excel;
using MedExpert.Web.Configuration;
using MedExpert.Web.Validators;
using MedExpert.Web.Services;
using MedExpert.Web.ViewModels.Account;
using MedExpert.Web.ViewModels.Analysis;
using MedExpert.Web.ViewModels.Import;
using MedExpert.Services.Interfaces;
using MedExpert.Services.Interfaces.Common;
using MedExpert.Services.Implementation;
using MedExpert.Services.Implementation.Common;

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

            var databaseConfig = Configuration.GetSection("Database").Get<DatabaseConfig>();
            var authorizationConfig = Configuration.GetSection("Authorization").Get<AuthorizationConfig>();
            services.AddSingleton(databaseConfig);
            services.AddSingleton(authorizationConfig);

            services.AddDbContext<MedExpertDataContext>(options => 
            {
                options.UseSqlServer(databaseConfig.ConnectionString);
            });
            
            EntityFrameworkManager.ContextFactory = _ =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<MedExpertDataContext>();
                optionsBuilder.UseSqlServer(databaseConfig.ConnectionString);
                return new MedExpertDataContext(optionsBuilder.Options);
            };

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
                        .WithOrigins("https://localhost:6001")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddScoped<IJwtGenerator, JwtGenerator>();
            services.AddScoped<UserManager<User>>();
            services.AddScoped<IEmailSender, EmailSender>();

            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(databaseConfig.ConnectionString)
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
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authorizationConfig.TokenKey));
            services.AddAuthentication((o) =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(
                    opt =>
                    {
                        opt.RequireHttpsMetadata = true;
                        opt.SaveToken = true;
                        opt.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = key,
                            ValidateAudience = false,
                            ValidateIssuer = false
                        };
                    });
            
            
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
            services.AddScoped<ISystemHealthCheckService, SystemHealthCheckService>();
            

            services.AddScoped(typeof(IRepository<>), typeof(SimpleRepository<>));

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