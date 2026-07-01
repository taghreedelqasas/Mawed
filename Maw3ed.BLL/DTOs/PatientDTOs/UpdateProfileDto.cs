using Maw3ed.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.PatientDTOs
{
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
    }
}
