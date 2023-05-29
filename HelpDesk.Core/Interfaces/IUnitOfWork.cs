using HelpDesk.Core.Entities;
using System;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository CustomerRepository { get; }
        IRepository<Empresa> BusinessRepository { get; }
        IDeviceRepository DeviceRepository { get; }
        IRepository<Piezas> PieceRepository { get; }
        IRepository<Seguimiento> TraceRepository { get; }
        ITicketRepository TicketRepository { get; }
        IUserRepository UserRepository { get; }

        void SaveChanges();
        Task SaveChangeAsync();
    }
}
