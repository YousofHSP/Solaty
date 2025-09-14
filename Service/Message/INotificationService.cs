using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Message
{
    public interface INotificationService
    {
        Task SendNotification(int userId, string title, int creatorUserId,CancellationToken ct);
        Task SendNotifications(string apiTokenCode, CancellationToken ct);

    }
}
