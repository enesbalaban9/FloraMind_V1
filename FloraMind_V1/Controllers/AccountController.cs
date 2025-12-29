using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Data;
using FloraMind_V1.Models;
using FloraMind_V1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using FloraMind_V1.Helpers;
using FloraMind_V1.Models.Login; // Statik şifreleme metotları buradan gelir

namespace FloraMind_V1.Controllers
{
    public class AccountController : Controller
    {
        private readonly FloraMindDbContext _context;
        private readonly IUserService _userService;

        // Tek ve birleşik constructor (DI için)
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
                Name = model.UserName,
                Email = model.Email,
                PasswordHash = passwordHash,
                RegistrationDate = DateTime.UtcNow,
                LastLoginDate = null,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Yeni kayıttan sonra otomatik oturum açma
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

            // Kullanıcı ve Şifre Kontrolü (Statik VerifyPassword kullanıldı)
            if (user == null || !SecurityHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Geçersiz email veya şifre");
                return View(model);
            }

            // Hesap Askıya Alma Kontrolü
            if (user.IsBanned)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız yönetici tarafından askıya alınmıştır.");
                return View(model);
            }

            await _userService.UpdateLastLoginDateAsync(user.UserID);

            // CLAIMS TABANLI OTURUM AÇMA
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
            // Cookie tabanlı oturumu kapatma
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if(!ModelState.IsValid)
            { 
                return View(model); 
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == model.Email);
            if (user != null)
            {
                var random = new Random();
                var VerificationCode = random.Next(100000, 999999).ToString();

                var forgotPasswordEntry = new ForgotPassword
                {
                    Email = user.Email,
                    VerificationCode = VerificationCode,
                    ExpirationDate = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false,
                    UserID = user.UserID
                };

                _context.ForgottenPasswords.Add(forgotPasswordEntry);
                await _context.SaveChangesAsync();

                return RedirectToAction("VerifyCode", new { email = model.Email });
            }

            return RedirectToAction("VerifyCode", new { email = model.Email });
        }
    }

}