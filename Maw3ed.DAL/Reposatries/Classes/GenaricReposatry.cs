using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Reposatries.Classes
{
    public class GenaricReposatry<TEntity> : IGenaricReposatry<TEntity> where TEntity : class
    {
        private readonly AppDbContext _dbcontext;

        public GenaricReposatry(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public void Add(TEntity tentity)
        {
            _dbcontext.Set<TEntity>().Add(tentity);
        }

        public void Delete(TEntity tentity)
        {
            _dbcontext.Set<TEntity>().Remove(tentity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync( // تعديل هنا
            Expression<Func<TEntity, bool>>? condition = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbcontext.Set<TEntity>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (condition != null)
            {
                query = query.Where(condition);
            }

            return await query.AsNoTracking().ToListAsync(); // تعديل هنا
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? condition = null) // تعديل هنا
        {
            return await GetAllAsync(condition, Array.Empty<Expression<Func<TEntity, object>>>());
        }

        public async Task<TEntity?> GetByIdAsync(int id) // تعديل هنا
         => await _dbcontext.Set<TEntity>().FindAsync(id);

        public void Update(TEntity tentity)
        {
            _dbcontext.Set<TEntity>().Update(tentity);
        }
    }
}