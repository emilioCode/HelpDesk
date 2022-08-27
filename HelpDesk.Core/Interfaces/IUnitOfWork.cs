using HelpDesk.Core.Entities;
using System;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Cliente> CustomerRepository { get; }
        IRepository<Empresa> BusinessRepository { get; }
        IRepository<Equipo> ComponentRepository { get; }
        IRepository<Piezas> PieceRepository { get; }
        IRepository<Seguimiento> TraceRepository { get; }
        IRepository<Solicitud> RequestRepository { get; }
        IUserRepository UserRepository { get; }

        void SaveChanges();
        Task SaveChangeAsync();
    }
}
