// Maw3ed.BLL/DTOs/Wallet/WithdrawRequestAdminDto.cs
namespace Maw3ed.BLL.DTOs.Wallet
{
    public class WithdrawRequestAdminDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = "";
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}