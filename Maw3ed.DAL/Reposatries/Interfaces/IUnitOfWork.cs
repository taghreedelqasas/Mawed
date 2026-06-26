using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL.Reposatries.Interfaces
{
    public interface IUnitOfWork
    {
        IGenaricReposatry<TEntity> GetReposatry<TEntity>() where TEntity :class;

        int SaveChanges();
    }
}
