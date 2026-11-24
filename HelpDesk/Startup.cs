
using HelpDesk.Infrastructure.Extensions;
using FluentValidation;
using HelpDesk.Infrastructure.Filters;
using HelpDesk.Infrastructure.Validators;
using HelpDesk.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
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

            // Behind a reverse proxy (see docs/DEPLOYMENT.md) the address and the scheme of the visitor arrive in X-Forwarded-For and
            // X-Forwarded-Proto. They are only trusted when ForwardedHeaders:Enabled is true, because anyone who can reach the app
            // directly could otherwise write them. Turn it on only when the app can be reached from the proxy alone.
            if (Configuration.GetValue<bool>("ForwardedHeaders:Enabled"))
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    // The proxy is another container, not the loopback address the defaults trust.
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });
            }

            services.AddServices();
            // The public tracking page: the settings, and a limit of calls per minute from one address.
            var tracking = new HelpDesk.Core.Services.TrackingOptions();
            Configuration.GetSection("Tracking").Bind(tracking);
            if (tracking.DaysAfterApproval < 1) tracking.DaysAfterApproval = 30;
            if (tracking.RequestsPerMinute < 1) tracking.RequestsPerMinute = 30;
            services.AddSingleton(tracking);
            services.AddRateLimiter(limiter =>
            {
                limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                limiter.AddPolicy("tracking", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = tracking.RequestsPerMinute, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            });
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, /*IHostingEnvironment*/IWebHostEnvironment env)
        {
            if (Configuration.GetValue<bool>("ForwardedHeaders:Enabled")) app.UseForwardedHeaders();

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
            app.UseRateLimiter();
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
