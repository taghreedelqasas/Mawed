using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL.Reposatries.Classes
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbcontext;

        public UnitOfWork(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }
        private readonly Dictionary<Type, Object> _reposatries = new Dictionary<Type, object>();

        public IGenaricReposatry<TEntity> GetReposatry<TEntity>() where TEntity : class
        {

            var EntityType = typeof(TEntity);
            if (_reposatries.TryGetValue(EntityType, out var Repo))
                return (IGenaricReposatry<TEntity>)Repo;

            var newRepo = new GenaricReposatry<TEntity>(_dbcontext);
            _reposatries[EntityType] = newRepo;
            return newRepo;


        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbcontext.SaveChangesAsync();
        }
    }

}
