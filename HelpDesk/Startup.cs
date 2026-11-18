
using HelpDesk.Infrastructure.Extensions;
using FluentValidation;
using HelpDesk.Infrastructure.Filters;
using HelpDesk.Infrastructure.Validators;
using HelpDesk.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace HelpDesk
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // index.html is checked with the server on every load, so a new deployment is picked up at once. The bundles carry a
        // hash of their content in the file name, so they can be kept for a year without ever being out of date.
        // The length of the hash depends on the version of Angular (20 characters up to 12, 16 from 13) and, with the esbuild builder, the hash comes after a dash: main-4ZCH2Y4H.js (8 upper-case letters and digits).
        private static void SetSpaCacheHeaders(StaticFileResponseContext context)
        {
            var hashed = Regex.IsMatch(context.File.Name, @"(\.[0-9a-f]{8,32}|-[0-9A-Z]{8})\.(js|css)$");
            context.Context.Response.Headers[HeaderNames.CacheControl] = hashed ? "public,max-age=31536000,immutable" : "no-cache";
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {     
            services.AddDbContexts(Configuration);
            services.AddSecurityServices(Configuration);
            services.AddHelpDeskAuthentication(Configuration);

            services.AddSignalR();
            services.AddRazorPages();
            services
                .AddControllersWithViews(options =>
                {
                    options.Filters.Add<GlobalExceptionFilter>();
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    //options.SuppressModelStateInvalidFilter = true; //to disable the ModelState in an ApiController
                });

            services
                .AddMvc(options =>
                {
                    options.Filters.Add<ValidationFilter>();
                    // The rules of the validators (Validators folder) are applied to every body the API receives
                    options.ModelValidatorProviders.Add(new FluentValidationModelValidatorProvider());
                });
            services.AddValidatorsFromAssemblyContaining<CustomerValidator>(ServiceLifetime.Transient);

            services.AddServices();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, /*IHostingEnvironment*/IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseDefaultFiles();
            app.UseSpaStaticFiles(new StaticFileOptions { OnPrepareResponse = SetSpaCacheHeaders });

            //app.UseSignalR(x =>
            //{
            //    x.MapHub<Hubs.HelpDeskHub>("/hub");
            //});
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller}/{action=Index}/{id?}");
            //});
            app.UseRouting();
            // The SPA is served by this same application, so CORS is only needed when other origins are configured.
            var allowedOrigins = (Configuration["Cors:AllowedOrigins"] ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (allowedOrigins.Length > 0)
            {
                app.UseCors(options =>
                {
                    options.WithOrigins(allowedOrigins);
                    options.AllowAnyHeader();
                    options.AllowAnyMethod();
                });
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}"//,
                    /*defaults: new { controller="App", Action="Index" }*/);
                endpoints.MapHub<Hubs.HelpDeskHub>("/hub");
            });
           

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";
                spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions { OnPrepareResponse = SetSpaCacheHeaders };

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
