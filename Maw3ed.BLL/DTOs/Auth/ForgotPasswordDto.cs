using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = default!;
        public string ClientBaseUrl { get; set; } = default!;
    }
}
