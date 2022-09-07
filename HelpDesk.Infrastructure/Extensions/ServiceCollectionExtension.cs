using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Services;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Infrastructure.Interfaces;
using HelpDesk.Infrastructure.Repositories;
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

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IBusinessService, BusinessService>();
            services.AddTransient<ISecurityService, Security>();
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
