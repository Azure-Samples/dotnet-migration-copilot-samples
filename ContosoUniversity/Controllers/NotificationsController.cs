using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContosoUniversity.Services;
using ContosoUniversity.Data;

namespace ContosoUniversity.Controllers
{
    public class NotificationsController : BaseController
    {
        public NotificationsController(SchoolContext context, NotificationService notificationService)
            : base(context, notificationService)
        {
        }

        // GET: api/notifications - Get pending notifications for admin
        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            try
            {
                var notifications = await notificationService.ReceiveNotificationsAsync(10);
                return Json(new {
                    success = true,
                    notifications,
                    count = notifications.Count
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving notifications: {ex.Message}");
                return Json(new { success = false, message = "Error retrieving notifications" });
            }
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