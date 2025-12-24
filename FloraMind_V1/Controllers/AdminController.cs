using Microsoft.AspNetCore.Mvc;
using FloraMind_V1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using FloraMind_V1.Models;
using FloraMind_V1.Data;
namespace FloraMind_V1.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    private readonly FloraMindDbContext _context;
    private readonly IUserService _userService;

    public AdminController(IUserService userService, FloraMindDbContext context) // Constructor nasıl?
    {
        _userService = userService;
        _context = context;
    }




    [HttpGet]
        public async Task<IActionResult> UserList()
        {
            var users = await _userService.GetAllUsersAsync();

            return View("~/Views/Admin/UserList.cshtml", users);
        }

        // 2. Rol Yükseltme Metodu (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(int id)
        {
            await _userService.UpdateUserRole(id, "Admin");
            return RedirectToAction(nameof(UserList));
        }

    public async Task<IActionResult> UserList(string searchString)
    {
        
        ViewData["CurrentFilter"] = searchString;

        var users = await _userService.GetUsersAsync(searchString);

        
        return View(users);
    }


    //kullanıcı hesaplarını askıya alma işlemleri

    [HttpPost]
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> ToggleBan(int id, bool banStatus)
    {
        // Hedef Kullanıcının Rolünü Alma
        var targetUser = await _userService.GetUserByIdAsync(id);

        if (targetUser == null)
        {
            TempData["ErrorMessage"] = "Belirtilen ID'ye sahip kullanıcı bulunamadı.";
            return RedirectToAction(nameof(UserList));
        }

        // Başka bir Admin'i Yasaklamayı Engelleme
        if (targetUser.Role == "Admin")
        {
            TempData["ErrorMessage"] = "Yönetici hesabını yasaklama/değiştirme yetkiniz yoktur.";
            return RedirectToAction(nameof(UserList));
        }

      

        // İşlemi Gerçekleştirme
        await _userService.ToggleBanStatusAsync(id, banStatus);

        // Başarı Mesajı Gönderme
        string action = banStatus ? "askıya alındı (yasaklandı)" : "tekrar aktif edildi";
        TempData["SuccessMessage"] = $"{targetUser.Name} adlı kullanıcı başarıyla {action}.";

        // Kullanıcı listesi sayfasına geri dönme
        return RedirectToAction(nameof(UserList));
    }
}


