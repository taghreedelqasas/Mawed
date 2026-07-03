using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public class LoginDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
