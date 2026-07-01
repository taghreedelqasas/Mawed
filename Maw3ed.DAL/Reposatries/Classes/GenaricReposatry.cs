using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.DAL.Reposatries.Classes
{
    public class GenaricReposatry<TEntity> : IGenaricReposatry<TEntity> where TEntity : class
    {
        private readonly AppDbContext _context;

        public GenaricReposatry(AppDbContext context)
        {
            _context = context;
        }

        public TEntity? GetById(int id)
            => _context.Set<TEntity>().Find(id);

        public async Task<TEntity?> GetByIdAsync(int id)
            => await _context.Set<TEntity>().FindAsync(id);

        public async Task<IEnumerable<TEntity>> GetAllAsync()
            => await _context.Set<TEntity>().AsNoTracking().ToListAsync();

        public void Add(TEntity entity)
            => _context.Set<TEntity>().Add(entity);

        public void Update(TEntity entity)
            => _context.Set<TEntity>().Update(entity);

        public void Delete(TEntity entity)
            => _context.Set<TEntity>().Remove(entity);
    }
}
