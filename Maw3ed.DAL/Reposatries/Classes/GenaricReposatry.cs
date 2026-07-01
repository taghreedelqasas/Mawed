using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Maw3ed.DAL.Reposatries.Classes
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>
     where TEntity : class
    {
        private readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TEntity?> GetByIdAsync(int id)
            => await _context.Set<TEntity>().FindAsync(id);

        public async Task<IEnumerable<TEntity>> GetAllAsync()
            => await _context.Set<TEntity>().AsNoTracking().ToListAsync();

        // Expression عشان الفلتر يتعمل في الـ Database مش في الميموري
        public async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> condition)
            => await _context.Set<TEntity>()
                .AsNoTracking()
                .Where(condition)
                .ToListAsync();

        public async Task AddAsync(TEntity entity)
            => await _context.Set<TEntity>().AddAsync(entity);

        public void Update(TEntity entity)
            => _context.Set<TEntity>().Update(entity);

        public void Delete(TEntity entity)
            => _context.Set<TEntity>().Remove(entity);
    }

}
