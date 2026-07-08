// Maw3ed.BLL/DTOs/Wallet/WalletResponseDto.cs
namespace Maw3ed.BLL.DTOs.Wallet
{
    public class WalletResponseDto
    {
        public decimal Balance { get; set; }
        public decimal PendingBalance { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}