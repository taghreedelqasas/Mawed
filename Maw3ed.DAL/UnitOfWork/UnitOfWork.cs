using Maw3ed.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbcontext;
        private IAuthRepository? _authRepository;

        public UnitOfWork(AppDbContext dbcontext, IAuthRepository authRepository)
        {
            _dbcontext = dbcontext;
            _authRepository = authRepository;
        }
        private readonly Dictionary<Type, Object> _repositories = new Dictionary<Type, object>();

        public IAuthRepository AuthRepository => _authRepository!;

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {

            var EntityType = typeof(TEntity);
            if (_repositories.TryGetValue(EntityType, out var Repo))
                return (IGenericRepository<TEntity>)Repo;

            var newRepo = new GenericRepository<TEntity>(_dbcontext);
            _repositories[EntityType] = newRepo;
            return newRepo;


        }

        public int SaveChanges()
        {
            return _dbcontext.SaveChanges();
        }
    }
}
