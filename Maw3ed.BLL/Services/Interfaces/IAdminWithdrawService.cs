// Maw3ed.BLL/Services/Interfaces/IAdminWithdrawService.cs
using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Wallet;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IAdminWithdrawService
    {
        Task<IEnumerable<WithdrawRequestAdminDto>> GetPendingRequestsAsync();
        Task<ServiceResult> ApproveAsync(int requestId);
        Task<ServiceResult> RejectAsync(int requestId);
    }
}