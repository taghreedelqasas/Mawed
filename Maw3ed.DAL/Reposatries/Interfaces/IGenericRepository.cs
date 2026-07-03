using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Maw3ed.DAL
{
     
        public interface IGenericRepository<TEntity> where TEntity : class
        {
            //GetALL
            IEnumerable<TEntity> GetAll(Func<TEntity, bool>? Condition = null);
        //Get 34an solve doctor manager include problem
        public IQueryable<TEntity> GetAllWithIncludes(params Expression<Func<TEntity, object>>[] includes);

            //getById

            TEntity? GetById(int id);
            //Add 

            void Add(TEntity tentity);
            //update
            void Update(TEntity tentity);
            //Delete
            void Delete(TEntity tentity);
        }
    }

