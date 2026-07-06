// Maw3ed.BLL/Services/Classes/AdminWithdrawService.cs
using Maw3ed.BLL.Common;
using Maw3ed.BLL.DTOs.Wallet;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Maw3ed.BLL.Services.Classes
{
    public class AdminWithdrawService : IAdminWithdrawService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public AdminWithdrawService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IEnumerable<WithdrawRequestAdminDto>> GetPendingRequestsAsync()
        {
            return await _context.WithdrawRequests
                .Include(r => r.Doctor).ThenInclude(d => d.User)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.CreatedAt)
                .Select(r => new WithdrawRequestAdminDto
                {
                    Id = r.Id,
                    DoctorName = r.Doctor.User.FirstName + " " + r.Doctor.User.LastName,
                    Amount = r.Amount,
                    Method = r.Method,
                    AccountNumber = r.AccountNumber,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ServiceResult> ApproveAsync(int requestId)
        {
            var request = await _context.WithdrawRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
                return new(false, "Request not found.", ServiceError.NotFound);

            if (request.Status != "Pending")
                return new(false, "Request already processed.", ServiceError.Conflict);

            var wallet = (await _unitOfWork.GetRepository<DoctorWallet>()
                .GetAllAsync(w => w.DoctorId == request.DoctorId)).FirstOrDefault();

            request.Status = "Approved";

            // الفلوس دي بقت خارج النظام فعليًا (المفروض الأدمن حوّلها يدويًا بنكيًا/InstaPay)
            if (wallet != null)
            {
                wallet.PendingBalance -= request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.GetRepository<DoctorWallet>().Update(wallet);
            }

            _unitOfWork.GetRepository<WithdrawRequest>().Update(request);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Withdraw approved.");
        }

        public async Task<ServiceResult> RejectAsync(int requestId)
        {
            var request = await _context.WithdrawRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
                return new(false, "Request not found.", ServiceError.NotFound);

            if (request.Status != "Pending")
                return new(false, "Request already processed.", ServiceError.Conflict);

            var wallet = (await _unitOfWork.GetRepository<DoctorWallet>()
                .GetAllAsync(w => w.DoctorId == request.DoctorId)).FirstOrDefault();

            request.Status = "Rejected";

            // السحب اتلغى، الفلوس ترجع تاني للرصيد المتاح
            if (wallet != null)
            {
                wallet.PendingBalance -= request.Amount;
                wallet.Balance += request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.GetRepository<DoctorWallet>().Update(wallet);
            }

            _unitOfWork.GetRepository<WithdrawRequest>().Update(request);
            await _unitOfWork.SaveChangesAsync();

            return new(true, "Withdraw rejected.");
        }
    }
}