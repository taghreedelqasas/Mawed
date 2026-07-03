using Maw3ed.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
   
    public interface IUnitOfWork
    {
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        // Auth is exposed separately because it wraps Identity managers
        // (UserManager/RoleManager/SignInManager), not a normal EF entity.
        IAuthRepository AuthRepository { get; }

        int SaveChanges();
    }
}
