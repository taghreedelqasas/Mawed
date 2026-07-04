using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public class ConfirmEmailDto
    {
        public string UserId { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}
