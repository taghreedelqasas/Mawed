using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public class ResetPasswordDto
    {
        public string UserId { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
