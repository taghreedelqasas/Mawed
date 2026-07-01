using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Reposatries.Interfaces
{
    public interface IGenaricReposatry<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? condition = null); // تعديل هنا

        Task<TEntity?> GetByIdAsync(int id); // تعديل هنا

        void Add(TEntity tentity);
        void Update(TEntity tentity);
        void Delete(TEntity tentity);

        Task<IEnumerable<TEntity>> GetAllAsync( // تعديل هنا
            Expression<Func<TEntity, bool>>? condition = null,
            params Expression<Func<TEntity, object>>[] includes);
    }
}