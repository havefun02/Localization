using Localization.Services;
using Localization.Interfaces;

using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;

namespace Localization
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(opt => opt.ResourcesPath = "Resources");
            services.AddTransient<IGreetingService,GreetingService>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("vi"),
            };

            var options = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(culture: "vi"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            options.RequestCultureProviders = new[]
            {
                new RouteDataRequestCultureProvider(){Options = options}
            };
            services.AddSingleton(options);
            services.AddMvc();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseMiddleware<Inspect>();
            app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;
                var statusCode = response.StatusCode;

                if (statusCode == 404)
                {
                    response.ContentType = "text/html";
                    await response.SendFileAsync("wwwroot/notFoundPage.html");
                }
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{culture}/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}