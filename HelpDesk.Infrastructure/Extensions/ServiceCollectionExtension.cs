using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Services;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Infrastructure.Interfaces;
using HelpDesk.Infrastructure.Repositories;
using HelpDesk.Infrastructure.Security;
using HelpDesk.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<HelpDeskDBContext>(option =>
                option.UseSqlServer(configuration.GetConnectionString("HelpDesk"))
            );
            return services;
        }

        // Password hashing, reversible encryption for stored secrets and sign-in attempt tracking.
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            var secretsKey = configuration["Secrets:Key"];
            if (string.IsNullOrEmpty(secretsKey) || secretsKey.Length < 32)
            {
                throw new System.InvalidOperationException("Secrets:Key must be set to at least 32 characters (environment variable Secrets__Key).");
            }
            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            services.AddSingleton<ISecretProtector>(new AesSecretProtector(secretsKey));
            services.AddSingleton<ILoginAttemptTracker>(new InMemoryLoginAttemptTracker(
                ReadInt(configuration, "Auth:MaxFailedAttempts", 5),
                ReadInt(configuration, "Auth:LockoutMinutes", 15)));
            return services;
        }

        private static int ReadInt(IConfiguration configuration, string key, int fallback)
        {
            return int.TryParse(configuration[key], out var value) && value > 0 ? value : fallback;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IBusinessService, BusinessService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<ITicketService, TicketService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IDeviceService, DeviceService>();
            services.AddTransient<ITraceService, TraceService>();
            services.AddTransient<IPieceService, PieceService>();
            services.AddTransient<IEventService, EventService>();

            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
