// Maw3ed.BLL/Services/Interfaces/IWithdrawService.cs
using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Wallet;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IWithdrawService
    {
        Task<ServiceResult> CreateWithdrawRequestAsync(string doctorUserId, WithdrawRequestDto dto);
    }
}