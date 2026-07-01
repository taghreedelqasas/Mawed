using Maw3ed.DAL.Reposatries.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maw3ed.DAL.DoctorDev.DoctorRepositoryInterfaces; 
namespace Maw3ed.DAL.DoctorDev.DoctorRepositoryClasses
{
    internal class DoctorRepository:GenaricReposatry<Doctor> , IDoctorRepository
    {

        private readonly AppDbContext _context;
           public   DoctorRepository(AppDbContext context):base(context) {
        
            _context = context;
        }

    }
}
