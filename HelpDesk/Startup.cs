using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HelpDesk.Models;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Controllers;
using Microsoft.Extensions.Hosting;

namespace HelpDesk
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
            var Server = Security.Decrypting( Configuration.GetConnectionString("Server") );//"Server": "ASUS" ó "Server": "127.0.0.1"
            var Database = Security.Decrypting( Configuration.GetConnectionString("Database") ); //"Database": "HelpDeskDB"
            var User = Security.Decrypting( Configuration.GetConnectionString("User") ); //"User": "sa"
            var Password = Security.Decrypting(Configuration.GetConnectionString("Password"));

            string connectionString = $"Server={Server};Database={Database};User Id={User};Password={Password};";
            //conection through appsettings.json
            services.AddDbContext<HelpDeskDBContext>(option =>
                option.UseSqlServer(connectionString));

            services.AddSignalR();

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddControllersWithViews();
            //now, i creating a scope with the dbLibraryContext
            services.AddScoped<HelpDeskDBContext, HelpDeskDBContext>();

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

            app.UseCors(options =>
            {
                options.AllowAnyOrigin();
                options.AllowAnyHeader();
                options.AllowAnyMethod();
            });
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
            app.UseEndpoints(endpoints =>
            {
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
