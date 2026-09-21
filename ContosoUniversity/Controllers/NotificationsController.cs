using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Services;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers
{
    public class NotificationsController : BaseController
    {
        // wwwroot/Scripts/notifications.js reads PascalCase properties (Operation, CreatedAt, ...);
        // preserve that casing instead of System.Text.Json's default camelCase.
        private static readonly JsonSerializerOptions PascalCaseJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };

        // GET: api/notifications - Get pending notifications for admin
        [HttpGet]
        public JsonResult GetNotifications()
        {
            var notifications = new List<Notification>();
            
            try
            {
                // Read all available notifications from the queue
                Notification notification;
                while ((notification = notificationService.ReceiveNotification()) != null)
                {
                    notifications.Add(notification);
                    
                    // Limit to prevent overwhelming the UI
                    if (notifications.Count >= 10)
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving notifications: {ex.Message}");
                return Json(new { success = false, message = "Error retrieving notifications" }, PascalCaseJsonOptions);
            }

            return Json(new { 
                success = true, 
                notifications = notifications,
                count = notifications.Count 
            }, PascalCaseJsonOptions);
        }

        // POST: api/notifications/mark-read
        [HttpPost]
        public JsonResult MarkAsRead(int id)
        {
            try
            {
                notificationService.MarkAsRead(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
                return Json(new { success = false, message = "Error updating notification" });
            }
        }

        // GET: Notifications/Index - Admin notification dashboard
        public IActionResult Index()
        {
            return View();
        }
    }
}
