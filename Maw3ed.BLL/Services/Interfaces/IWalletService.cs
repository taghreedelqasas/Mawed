// Maw3ed.BLL/Services/Interfaces/IWalletService.cs
using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Wallet;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface IWalletService
    {
        Task<ServiceResult<WalletResponseDto>> GetWalletAsync(string doctorUserId);
        Task<ServiceResult<IEnumerable<WalletTransactionDto>>> GetTransactionsAsync(string doctorUserId);
    }
}