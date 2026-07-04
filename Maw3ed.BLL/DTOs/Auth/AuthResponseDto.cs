using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public class AuthResponseDto
    {
        public bool IsAuthenticated { get; set; }

        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        //public string Message { get; set; }

        public string? Token { get; set; }
        public DateTime? ExpiresOn { get; set; }

        // Used to return register/login failures (wrong password, duplicate email, locked out...)
        public List<string> Errors { get; set; } = new List<string>();

        public static AuthResponseDto Fail(params string[] errors)
            => new AuthResponseDto { IsAuthenticated = false, Errors = new List<string>(errors) };
    }
}
