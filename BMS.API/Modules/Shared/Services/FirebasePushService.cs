using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace BMS.API.Modules.Shared.Services
{
    public class FirebasePushService : IFirebasePushService
    {
        private readonly ILogger<FirebasePushService> _logger;

        public FirebasePushService(ILogger<FirebasePushService> logger)
        {
            _logger = logger;
        }

        public async Task SendPushNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
                return;

            try
            {
                var message = new Message
                {
                    Token = fcmToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Successfully sent message: {response}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FCM message to token: {Token}", fcmToken);
            }
        }

        public async Task SendMulticastNotificationAsync(IEnumerable<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null)
        {
            var validTokens = fcmTokens?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            if (validTokens == null || !validTokens.Any())
                return;

            try
            {
                // Firebase multicast limit is 500 per call. We'll chunk it.
                const int MAX_TOKENS_PER_CALL = 500;
                
                for (int i = 0; i < validTokens.Count; i += MAX_TOKENS_PER_CALL)
                {
                    var chunk = validTokens.Skip(i).Take(MAX_TOKENS_PER_CALL).ToList();
                    
                    var message = new MulticastMessage
                    {
                        Tokens = chunk,
                        Notification = new Notification
                        {
                            Title = title,
                            Body = body
                        },
                        Data = data
                    };

                    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                    _logger.LogInformation("Successfully sent multicast message. Success count: {Count}", response.SuccessCount);
                    
                    if (response.FailureCount > 0)
                    {
                        foreach (var resp in response.Responses.Where(r => !r.IsSuccess))
                        {
                            _logger.LogWarning("Failed to send to a token. Error: {Error}", resp.Exception?.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FCM multicast message.");
            }
        }
    }
}
