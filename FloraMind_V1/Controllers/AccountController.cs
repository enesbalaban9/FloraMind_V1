using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Data;
using FloraMind_V1.Models;
using FloraMind_V1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using FloraMind_V1.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace FloraMind_V1.Controllers
{
    public class AccountController : Controller
    {
        private readonly FloraMindDbContext _context;
        private readonly IUserService _userService;

        public AccountController(FloraMindDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email zaten kayıtlı");
                return View(model);
            }

            string passwordHash = SecurityHelper.HashPassword(model.Password);

            var user = new User
            {
                Name = model.UserName, // Register modelinde UserName kullanılıyor, burası doğru
                Email = model.Email,
                PasswordHash = passwordHash,
                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = null,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !SecurityHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Geçersiz email veya şifre");
                return View(model);
            }

            if (user.IsBanned)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız yönetici tarafından askıya alınmıştır.");
                return View(model);
            }

            await _userService.UpdateLastLoginDateAsync(user.UserID);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // --- HESABIM KISMI (DÜZELTİLDİ) ---

        [Authorize]
        public async Task<IActionResult> Hesabim()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login");

            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                // DÜZELTME: model.UserName yerine model.Name kullanıldı
                Name = user.Name,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hesabim(EditProfileViewModel model)
        {
            // Password alanı zorunlu olmadığı için ModelState validasyonunu manipüle edebiliriz
            // Veya modelde Password nullable olduğu için sorun çıkmayacaktır.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login");

            // DÜZELTME: model.UserName yerine model.Name kullanıldı
            user.Name = model.Name;
            user.Email = model.Email;

            // Şifre alanı boş değilse güncelle
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = SecurityHelper.HashPassword(model.Password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Bilgileriniz başarıyla güncellendi!";
            return View(model);
        }
    }
}