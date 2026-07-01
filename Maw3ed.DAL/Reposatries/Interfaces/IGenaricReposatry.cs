namespace Maw3ed.DAL.Reposatries.Interfaces
{
    public interface IGenaricReposatry<TEntity> where TEntity : class
    {
        TEntity?  GetById(int id);
        void      Add(TEntity entity);
        void      Update(TEntity entity);
        void      Delete(TEntity entity);

        Task<TEntity?>             GetByIdAsync(int id);
        Task<IEnumerable<TEntity>> GetAllAsync();
    }
}
