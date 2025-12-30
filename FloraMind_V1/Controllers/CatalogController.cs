using Microsoft.AspNetCore.Mvc;
using FloraMind_V1.Models;
using FloraMind_V1.Data;
using Microsoft.EntityFrameworkCore;

namespace FloraMind_V1.Controllers
{
    public class CatalogController : Controller
    {
        private readonly FloraMindDbContext _context;

        public CatalogController(FloraMindDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ShowCatalog()
        {
            var plants = _context.Plants
                  .Include(p => p.Contents)
                  .ToList();
            return View(plants);
        }

                                                                                  // ---  BAŞLANGIÇ ---      
        public IActionResult Details(int id)
        {
            
            var plant = _context.Plants.FirstOrDefault(p => p.PlantID == id);

            if (plant == null)
            {
                return RedirectToAction("ShowCatalog");
            }           
            return View(plant);
        }
                                                                 /// KATALOG VE BİTKİLEİRM KISMINDAKİ BİTKİ BAĞLANTISI 

        public async Task<IActionResult> UserProfileDetails(int id)
        {
            var UserDetails = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == id);
            return View();
        }

        public async Task<IActionResult> ChangeUserName()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToMyPlants(int id)
        {
            // 1. Katalogdan bitkiyi bul
            var catalogPlant = await _context.Plants.FindAsync(id);
            if (catalogPlant == null)
            {
                return NotFound();
            }

            // 2. Mevcut kullanıcının ID'sini al
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 3. UserPlant nesnesini oluştur
            var newUserPlant = new UserPlant
            {
                PlantID = catalogPlant.PlantID,
                UserID = user.UserID,
                WateringIntervalHours = catalogPlant.DefaultWateringIntervalHours,
                DateAdopted = DateTime.Now
            };

            // 4. Sulama hesaplamasını yap
            newUserPlant.PerformWatering();

            // 5. Veritabanına ekle
            _context.UserPlants.Add(newUserPlant);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{catalogPlant.Name} başarıyla bitkilerime eklendi!";

            return RedirectToAction("Index", "UserPlants");
        }
    }
}