using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll();
        // The same rows as a query, to filter in the database instead of in memory.
        IQueryable<T> Query();
        Task<T> GetById(int id); 
        Task Add(T entity);
        void Update(T entity);
        Task Delete(int id);
    }
}
