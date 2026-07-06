// Maw3ed.BLL/Services/Classes/WalletService.cs
using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Wallet;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.BLL.Services.Classes
{
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;

        public WalletService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<WalletResponseDto>> GetWalletAsync(string doctorUserId)
        {
            var wallet = await _context.DoctorWallets
                .Include(w => w.Doctor)
                .FirstOrDefaultAsync(w => w.Doctor.UserId == doctorUserId);

            if (wallet is null)
                return new(false, "Wallet not found.", null, ServiceError.NotFound);

            return new(true, "OK", new WalletResponseDto
            {
                Balance = wallet.Balance,
                PendingBalance = wallet.PendingBalance,
                UpdatedAt = wallet.UpdatedAt
            });
        }

        public async Task<ServiceResult<IEnumerable<WalletTransactionDto>>> GetTransactionsAsync(string doctorUserId)
        {
            var doctorId = await _context.Doctors
                .Where(d => d.UserId == doctorUserId)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            if (doctorId == 0)
                return new(false, "Doctor not found.", null, ServiceError.NotFound);

            var transactions = await _context.WalletTransactions
                .Where(t => t.DoctorId == doctorId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    AppointmentId = t.AppointmentId,
                    Amount = t.Amount,
                    Type = t.Type,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new(true, "OK", transactions);
        }
    }
}