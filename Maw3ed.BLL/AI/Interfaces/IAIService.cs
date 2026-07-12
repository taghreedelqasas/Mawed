using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.AI.Interfaces
{
    public interface IAIService
    {
        Task<string> ChatAsync(string prompt);
    }
}
