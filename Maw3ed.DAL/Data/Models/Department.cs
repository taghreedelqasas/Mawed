using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Maw3ed.DAL
{
    public class Department
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<Doctor> Doctors { get; set; }
            = new List<Doctor>();
    }
}
