using FloraMind_V1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloraMind_V1.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly FloraMindDbContext _context;

        public NotificationsController(FloraMindDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Sadece giriş yapmış kullanıcının bildirimlerini getir
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);

            if (user == null) return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Where(n => n.UserID == user.UserID)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }
    }
}
