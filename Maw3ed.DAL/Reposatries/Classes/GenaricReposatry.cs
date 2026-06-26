using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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

        public IEnumerable<TEntity> GetAll(Func<TEntity, bool>? Condition = null)
        {
            if (Condition is null)
                return _dbcontext.Set<TEntity>().AsNoTracking().ToList();

            else
                return _dbcontext.Set<TEntity>().AsNoTracking().Where(Condition).ToList();
        }

        public TEntity? GetById(int id)
         => _dbcontext.Set<TEntity>().Find(id);

        public void Update(TEntity tentity)
        {
            _dbcontext.Set<TEntity>().Update(tentity);
        }
    }

}
