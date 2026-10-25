using AutoMapper;
using FluentValidation.AspNetCore;
using HelpDesk.Infrastructure.Extensions;
using HelpDesk.Infrastructure.Filters;
using HelpDesk.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

        // Dynamic assemblies (for example the ones a mocking library generates in tests) cannot be scanned.
        private static System.Reflection.Assembly[] ScannableAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToArray();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {     
            services.AddAutoMapper(ScannableAssemblies());
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
                })
                .AddFluentValidation(options =>
                {
                    options.RegisterValidatorsFromAssemblies(ScannableAssemblies());
                });

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
            app.UseSpaStaticFiles();

            //app.UseSignalR(x =>
            //{
            //    x.MapHub<Hubs.hub>("/hub");
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
                endpoints.MapHub<Hubs.hub>("/hub");
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
