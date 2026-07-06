// Maw3ed.BLL/DTOs/Wallet/WithdrawRequestDto.cs
namespace Maw3ed.BLL.DTOs.Wallet
{
    public class WithdrawRequestDto
    {
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";        // "InstaPay", "BankTransfer"...
        public string AccountNumber { get; set; } = "";
    }
}