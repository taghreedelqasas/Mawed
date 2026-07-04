using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Maw3ed.DAL.Reposatries.Interfaces
{
     
        public interface IGenericRepository<TEntity> where TEntity : class
        {
        Task<TEntity?> GetByIdAsync(int id);
        
        Task<IEnumerable<TEntity>> GetAllAsync();
        
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> condition);
        
        Task AddAsync(TEntity entity);
        
        void Update(TEntity entity);

        void Delete(TEntity entity);

        // الدوال الإضافية المأخوذة من develop لدعم الـ Includes والـ Filter
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? condition = null);

        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? condition = null,
            params Expression<Func<TEntity, object>>[] includes);
        }
    }

