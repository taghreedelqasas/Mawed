using Maw3ed.BLL.DTOs.Notifications;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.DAL;
using Maw3ed.DAL.Data.Models;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Classes
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public NotificationService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string userId,
            string title,
            string body,
            int? relatedEntityId = null)
        {
            // جيب الـ User عشان تاخد الـ Email بتاعه
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("المستخدم مش موجود");

            // احفظ الـ Notification في الـ Database أول
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                Status = NotificationStatus.Pending,
                RelatedEntityId = relatedEntityId,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<Notification>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // بعت الإيميل
            try
            {
                await SendEmailWithSmtpAsync(user.Email!, title, body);

                // لو الإيميل اتبعت تمام، حدّث الـ Status
                notification.Status = NotificationStatus.Sent;
                notification.SentAtUtc = DateTime.UtcNow;
                _unitOfWork.GetRepository<Notification>().Update(notification);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                // لو فيه مشكلة في الإيميل، سجّل Failed
                notification.Status = NotificationStatus.Failed;
                _unitOfWork.GetRepository<Notification>().Update(notification);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _unitOfWork.GetRepository<Notification>()
                .FindAsync(n => n.UserId == userId);

            return notifications
                .OrderByDescending(n => n.CreatedAtUtc)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    Status = n.Status,
                    IsRead = n.IsRead,
                    CreatedAtUtc = n.CreatedAtUtc,
                    SentAtUtc = n.SentAtUtc
                });
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notifications = await _unitOfWork.GetRepository<Notification>()
                .FindAsync(n => n.Id == notificationId && n.UserId == userId);

            var notification = notifications.FirstOrDefault();
            if (notification is null)
                return;

            notification.IsRead = true;
            _unitOfWork.GetRepository<Notification>().Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _unitOfWork.GetRepository<Notification>()
                .FindAsync(n => n.UserId == userId && !n.IsRead);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _unitOfWork.GetRepository<Notification>().Update(notification);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // الميثود اللي بتبعت الإيميل فعلاً
        private async Task SendEmailWithSmtpAsync(string toEmail, string subject, string body)
        {
            // جيب بيانات الإيميل من الـ appsettings.json
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await client.SendMailAsync(mailMessage);
        }
    }
}
