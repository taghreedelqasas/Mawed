namespace Maw3ed.DAL.Reposatries.Interfaces
{
    public interface IUnitOfWork
    {
        IGenaricReposatry<TEntity> GetReposatry<TEntity>() where TEntity : class;
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
