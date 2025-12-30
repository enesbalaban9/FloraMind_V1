using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Data;
using FloraMind_V1.Models;

namespace FloraMind_V1.Controllers
{
    // Kullanıcının oturum açmış olmasını zorunlu kılar.
    [Authorize]
    public class UserPlantsController : Controller
    {
        private readonly FloraMindDbContext _context;

        public UserPlantsController(FloraMindDbContext context)
        {
            _context = context;
        }

        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            
            return 1;
        }

        
        public async Task<IActionResult> Index()
        {
            var userId = GetLoggedInUserId();

            var userPlants = await _context.UserPlants
                                           .Where(up => up.UserID == userId)
                                           .Include(up => up.Plant) 
                                           .ThenInclude(p => p.Contents)
                                           .ToListAsync();

            return View(userPlants);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int plantId)
        {
            var userId = GetLoggedInUserId();

            var catalogPlant = await _context.Plants.FindAsync(plantId);
            if (catalogPlant == null)
            {
                return NotFound("Katalogda bu ID'ye sahip bir bitki bulunamadı.");
            }

            var existingUserPlant = await _context.UserPlants
                                            .AnyAsync(up => up.UserID == userId && up.PlantID == plantId);
            if (existingUserPlant)
            {
                TempData["Message"] = $"{catalogPlant.Name} zaten koleksiyonunuzda mevcut.";
                return RedirectToAction(nameof(Index));
            }

            var newUserPlant = new UserPlant
            {
                UserID = userId,
                PlantID = plantId,
                DateAdopted = DateTime.UtcNow,
                LastWatered = DateTime.UtcNow
            };

            _context.UserPlants.Add(newUserPlant);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{catalogPlant.Name} koleksiyonunuza başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WaterPlant(int id) 
        {
            var userId = GetLoggedInUserId();

            // Bitkiyi ve ilişkili Katalog bilgilerini (Plant) birlikte çekiyoruz 
            // ki sulama aralığını (DefaultWateringIntervalHours) görebilelim.
            var userPlant = await _context.UserPlants
                .Include(up => up.Plant)
                .FirstOrDefaultAsync(up => up.UserPlantID == id && up.UserID == userId);

            if (userPlant == null)
            {
                return NotFound("Sulama işlemi için uygun bir bitki kaydı bulunamadı.");
            }

            // 1. Son sulama zamanını şu an olarak ayarla
            userPlant.LastWatered = DateTime.Now;

            
            int aralik = userPlant.WateringIntervalHours > 0
                         ? userPlant.WateringIntervalHours
                         : (userPlant.Plant != null ? userPlant.Plant.DefaultWateringIntervalHours : 24);

            userPlant.NextWateringDate = DateTime.Now.AddHours(aralik);

            
            _context.Update(userPlant);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Bitkiniz sulandı, geri sayım yeniden başlatıldı!";
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int userPlantId)
        {
            var userId = GetLoggedInUserId();

            
            var userPlantToDelete = await _context.UserPlants
                .FirstOrDefaultAsync(up => up.UserPlantID == userPlantId && up.UserID == userId);

            if (userPlantToDelete != null)
            {
                _context.UserPlants.Remove(userPlantToDelete);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Bitki listenizden çıkarıldı.";
            }
            else
            {
                TempData["Error"] = "Bitki bulunamadı veya silinemedi.";
            }


            
            return RedirectToAction(nameof(Index));
        }
          
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetLoggedInUserId();

            
            var silinecekBitki = await _context.UserPlants
                .FirstOrDefaultAsync(up => up.UserPlantID == id && up.UserID == userId);

           
            if (silinecekBitki != null)
            {
                _context.UserPlants.Remove(silinecekBitki);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Bitki başarıyla silindi.";
            }

            
            return RedirectToAction(nameof(Index));
        }
    }
}