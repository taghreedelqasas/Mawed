using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Maw3ed.DAL
{
    internal class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class

    {
        private readonly AppDbContext _dbcontext;

        public GenericRepository(AppDbContext dbcontext)
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

        public IEnumerable<TEntity> GetAll(Func<TEntity, bool>? Condition = null)
        {
            if (Condition is null)
                return _dbcontext.Set<TEntity>().AsNoTracking().ToList();

            else
                return _dbcontext.Set<TEntity>().AsNoTracking().Where(Condition).ToList();
        }

        public IQueryable<TEntity> GetAllWithIncludes(params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbcontext.Set<TEntity>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public TEntity? GetById(int id)
         => _dbcontext.Set<TEntity>().Find(id);

        public void Update(TEntity tentity)
        {
            _dbcontext.Set<TEntity>().Update(tentity);
        }
    }
}
