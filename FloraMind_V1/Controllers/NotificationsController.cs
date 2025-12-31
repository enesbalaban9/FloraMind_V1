using System.Security.Claims;
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
            // Giriş yapan kullanıcının ID'sini en güvenli yoldan (Claim) alıyoruz
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            var notifications = await _context.Notifications
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }
    }
}