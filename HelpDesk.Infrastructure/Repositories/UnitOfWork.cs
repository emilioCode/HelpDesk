using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HelpDeskDBContext _context;

        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<Empresa> _businessRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IRepository<Piezas> _pieceRepository;
        private readonly IRepository<Seguimiento> _traceRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        public UnitOfWork(HelpDeskDBContext context)
        {
            _context = context;
        }

        public ICustomerRepository CustomerRepository => _customerRepository ?? new CustomerRepository(_context);

        public IRepository<Empresa> BusinessRepository => _businessRepository ?? new BaseRepository<Empresa>(_context);

        public IDeviceRepository DeviceRepository => _deviceRepository ?? new DeviceRepository(_context);

        public IRepository<Piezas> PieceRepository => _pieceRepository ?? new BaseRepository<Piezas>(_context);

        public IRepository<Seguimiento> TraceRepository => _traceRepository ?? new BaseRepository<Seguimiento>(_context);

        public ITicketRepository TicketRepository => _ticketRepository ?? new TicketRepository(_context);

        public IUserRepository UserRepository => _userRepository ?? new UserRepository(_context);

        public void Dispose()
        {
            if(_context != null)
            {
                _context.Dispose();
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
