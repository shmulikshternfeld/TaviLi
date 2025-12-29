using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using TaviLi.Application.Common.Interfaces;
using TaviLi.Domain.Entities;
using System.Text.Json;

namespace TaviLi.Infrastructure.Services
{
    public class FirebaseNotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly FirebaseMessaging? _messaging;

        public FirebaseNotificationService(
            IApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<FirebaseNotificationService> logger)
        {
            _context = context;
            _logger = logger;

            if (FirebaseApp.DefaultInstance == null)
            {
                GoogleCredential credential = null;
                
                // 1. Try to load from UserSecrets (Development) as a JSON string
                var jsonConfig = configuration["FirebaseConfig"];
                if (!string.IsNullOrEmpty(jsonConfig))
                {
                    try 
                    {
                        credential = GoogleCredential.FromJson(jsonConfig);
                         _logger.LogInformation("Firebase initialized using Configuration String (UserSecrets/EnvVar).");
                    }
                    catch (Exception ex)
                    {
                         _logger.LogError(ex, "Failed to parse FirebaseConfig from string.");
                    }
                }

                // 2. If valid credential not found, try file path (Production/Render)
                if (credential == null)
                {
                    // Render secret file path or fallback
                    var secretPath = "/etc/secrets/firebase-config.json"; 
                    if (File.Exists(secretPath))
                    {
                        credential = GoogleCredential.FromFile(secretPath);
                        _logger.LogInformation($"Firebase initialized using file at {secretPath}.");
                    }
                    else
                    {
                        _logger.LogWarning("Firebase credentials not found in string or default file path.");
                    }
                }

                if (credential != null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential
                    });
                }
            }

            // We wrap this in a try-catch or check because if App creation failed, this will throw
            try 
            {
                 _messaging = FirebaseMessaging.DefaultInstance;
            }
            catch
            {
                _messaging = null;
            }
        }

        public async Task RegisterDeviceTokenAsync(Guid userId, string token, string platform)
        {
            var existing = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == token);

            if (existing != null)
            {
                existing.LastUsed = DateTime.UtcNow;
            }
            else
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    UserId = userId,
                    Token = token,
                    Platform = platform
                });
            }

            await _context.SaveChangesAsync(default);
        }

        public async Task UnsubscribeDeviceTokenAsync(Guid userId, string token)
        {
            var existing = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == token);

            if (existing != null)
            {
                _context.DeviceTokens.Remove(existing);
                await _context.SaveChangesAsync(default);
            }
        }

        public async Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null, string? actionUrl = null, string type = "Info")
        {
            // 1. Save to Database (Always, even if push fails)
            var notification = new TaviLi.Domain.Entities.Notification
            {
                UserId = userId,
                Title = title,
                Body = body,
                ActionUrl = actionUrl,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync(default);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to save notification to DB. Inner Error: {Inner}", ex.InnerException?.Message);
                 throw; // Rethrow to let the controller handling it, but now we have logs.
            }

            // 2. Send Push to all user devices
            if (_messaging == null) 
            {
                _logger.LogWarning("Firebase Messaging is not initialized. Skipping Push.");
                return;
            }

            var tokens = await _context.DeviceTokens
                .Where(dt => dt.UserId == userId)
                .Select(dt => dt.Token)
                .ToListAsync();

            if (!tokens.Any()) return;

            // Prepare data dictionary first
            var finalData = data != null ? new Dictionary<string, string>(data) : new Dictionary<string, string>();
            finalData["url"] = actionUrl ?? "/";
            finalData["type"] = type;

            var message = new MulticastMessage()
            {
                Tokens = tokens,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = title,
                    Body = body
                },
                Data = finalData
            };

            try
            {
                var response = await _messaging.SendMulticastAsync(message);
                
                // Optional: Clean up invalid tokens
                if (response.FailureCount > 0)
                {
                    for (var i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            // Remove token if it's invalid
                            // We can choose to implement this cleanup logic
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending multicast notification to user {UserId}", userId);
            }
        }
    }
}
