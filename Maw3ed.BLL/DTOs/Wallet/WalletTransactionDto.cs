// Maw3ed.BLL/DTOs/Wallet/WalletTransactionDto.cs
namespace Maw3ed.BLL.DTOs.Wallet
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public int? AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}