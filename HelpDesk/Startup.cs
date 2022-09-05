using AutoMapper;
using FluentValidation.AspNetCore;
using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Services;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Infrastructure.Filters;
using HelpDesk.Infrastructure.Interfaces;
using HelpDesk.Infrastructure.Repositories;
using HelpDesk.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());    
            //conection through appsettings.json
            services.AddDbContext<HelpDeskDBContext>(option =>
                option.UseSqlServer(Configuration.GetConnectionString("HelpDesk"))
            );

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
                    options.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
                });

            //now, i creating a scope with the dbLibraryContext
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IBusinessService, BusinessService>();
            services.AddTransient<ISecurityService, Security>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ITicketService, TicketService>();
            services.AddTransient<IMailService, MailService>();

            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

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
