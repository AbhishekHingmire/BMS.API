using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMS.API.Modules.Shared.Services
{
    public interface IFirebasePushService
    {
        Task SendPushNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
        Task SendMulticastNotificationAsync(IEnumerable<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null);
    }
}
