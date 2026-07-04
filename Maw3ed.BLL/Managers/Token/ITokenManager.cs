using Maw3ed.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public interface ITokenManager
    {
        // Builds the JWT (claims: UserId, Email, UserName, Role[]) and returns
        // the signed token string along with its expiry.
        (string Token, System.DateTime ExpiresOn) GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
