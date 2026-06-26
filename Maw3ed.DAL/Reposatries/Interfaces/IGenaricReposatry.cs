using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL.Reposatries.Interfaces
{
    public interface IGenaricReposatry<TEntity>  where TEntity : class
    {
        //GetALL
        IEnumerable<TEntity> GetAll(Func<TEntity, bool>? Condition = null);

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
