using System;
using System.Collections.Generic;
using System.Text;

namespace Maw3ed.BLL
{
    public interface IAuthManager
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto dto);

        Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
