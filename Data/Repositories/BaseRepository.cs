using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Drum_Machine.Data.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual T GetById(int id) => _dbSet.Find(id);

        public virtual List<T> GetAll() => _dbSet.ToList();

        public virtual void Add(T entity) => _dbSet.Add(entity);

        public virtual void Update(T entity) => _dbSet.Update(entity);

        public virtual void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null) _dbSet.Remove(entity);
        }

        public virtual void Save() => _context.SaveChanges();
    }
}