using Maw3ed.BLL.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.Services.Interfaces
{
    public interface INotificationService
    {
        // بعت إيميل
        Task SendEmailAsync(string userId, string title, string body, int? relatedEntityId = null);

        // جيب كل الإشعارات بتاعة اليوزر
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
    }
}
