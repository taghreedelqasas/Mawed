using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Maw3ed.BLL.AI.Helpers
{
    public static class PdfTextExtractor
    {
        public static string ExtractText(Stream stream)
        {
            var text = new StringBuilder();

            using var document = PdfDocument.Open(stream);

            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }
    }
}
