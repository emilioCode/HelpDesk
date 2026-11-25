using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly HelpDeskDBContext _context;
        protected DbSet<T> _entities;
        public BaseRepository(HelpDeskDBContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            // Without an ORDER BY the database may return the rows in any order, and lists of equal items (the events of the
            // calendar, for one) change places between calls. By Id is the order they have always come in; a service that wants
            // another order sorts the result.
            return _entities.AsQueryable().AsNoTracking().OrderBy(x => x.Id);
        }

        public IQueryable<T> Query()
        {
            return _entities.AsNoTracking();
        }

        public async Task<T> GetById(int id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task Add(T entity)
        {
            await _entities.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _entities.Update(entity);
        }

        public async Task Delete(int id)
        {
            T entity = await GetById(id);
            _entities.Remove(entity);
        }
    }
}
