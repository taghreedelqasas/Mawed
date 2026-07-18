// Maw3ed.BLL/Services/Classes/WithdrawService.cs
using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Wallet;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.BLL.Services.Classes
{
    public class WithdrawService : IWithdrawService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public WithdrawService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ServiceResult> CreateWithdrawRequestAsync(string doctorUserId, WithdrawRequestDto dto)
        {
            var wallet = await _context.DoctorWallets
                .Include(w => w.Doctor)
                .FirstOrDefaultAsync(w => w.Doctor.UserId == doctorUserId);

            if (wallet is null)
                return new(false, "Wallet not found.", ServiceError.NotFound);

            if (dto.Amount <= 0)
                return new(false, "Invalid amount.", ServiceError.BadRequest);

            if (dto.Amount < 100)
                return new(false, "The minimum withdrawal amount is 100.", ServiceError.BadRequest);

            if (dto.Amount > wallet.Balance)
                return new(false, "Insufficient balance.", ServiceError.BadRequest);

            var request = new WithdrawRequest
            {
                DoctorId = wallet.DoctorId,
                Amount = dto.Amount,
                Method = dto.Method,
                AccountNumber = dto.AccountNumber,
                Status = "Pending"
            };

            // بنجمّد الفلوس فورًا عشان الدكتور ميقدرش يطلب سحب نفس الفلوس مرتين
            // وهو مستني موافقة الأدمن
            wallet.Balance -= dto.Amount;
            wallet.PendingBalance += dto.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.GetRepository<WithdrawRequest>().AddAsync(request);
            _unitOfWork.GetRepository<DoctorWallet>().Update(wallet);

            await _unitOfWork.SaveChangesAsync();

            return new(true, "Withdraw request submitted successfully.");
        }
    }
}