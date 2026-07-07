using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Maw3ed.BLL.AI.DTOs
{
    public class AnalyzeImageResponse
    {
        public string Explanation { get; set; } = string.Empty;
    }
}
