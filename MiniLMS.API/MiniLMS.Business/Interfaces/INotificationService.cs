using GenAlpha.Business.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenAlpha.Business.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> SendNotificationAsync(CreateNotificationDto dto);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
