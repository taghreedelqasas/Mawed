using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public abstract class AuditableEntity : IAuditableEntity
    {
        public DateTime CreatedAt { get ; set ; }
        public DateTime? UpdatedAt { get ; set ; }
        public string? CreatedBy { get ; set ; }
        public string? UpdatedBy { get ; set ; }
    }
}
