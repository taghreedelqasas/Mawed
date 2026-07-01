using Maw3ed.DAL.Reposatries.Interfaces;

namespace Maw3ed.DAL.Reposatries.Classes
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbcontext;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AppDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public IGenaricReposatry<TEntity> GetReposatry<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (_repositories.TryGetValue(type, out var repo))
                return (IGenaricReposatry<TEntity>)repo;

            var newRepo = new GenaricReposatry<TEntity>(_dbcontext);
            _repositories[type] = newRepo;
            return newRepo;
        }

        public int SaveChanges()         => _dbcontext.SaveChanges();
        public Task<int> SaveChangesAsync() => _dbcontext.SaveChangesAsync();
    }
}
