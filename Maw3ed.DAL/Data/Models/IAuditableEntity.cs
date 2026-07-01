using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.DAL
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }

        string? CreatedBy { get; set; }
        string? UpdatedBy { get; set; }
    }
}
