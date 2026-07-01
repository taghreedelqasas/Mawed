using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.DTOs.Notifications
{
    public class SendNotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public int? RelatedEntityId { get; set; }
    }
}
