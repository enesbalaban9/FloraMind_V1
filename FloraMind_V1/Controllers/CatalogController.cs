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

        [HttpGet]
        public async Task<IActionResult> Index(string searchName, string searchSpecies)
        {
            var plantsQuery = _context.Plants.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                plantsQuery = plantsQuery.Where(p => p.Name.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(searchSpecies))
            {
                plantsQuery = plantsQuery.Where(p => p.Species.Contains(searchSpecies));
            }

            var filteredList = await plantsQuery.ToListAsync();

            // Hata buradaydı: View adını açıkça "ShowCatalog" olarak belirtiyoruz
            return View("ShowCatalog", filteredList);
        }

        public IActionResult ShowCatalog()
        {
            var plants = _context.Plants
                  .Include(p => p.Contents)
                  .ToList();
            return View(plants);
        }

                                                                             
        public IActionResult Details(int id)
        {
            
            var plant = _context.Plants.FirstOrDefault(p => p.PlantID == id);

            if (plant == null)
            {
                return RedirectToAction("ShowCatalog");
            }           
            return View(plant);
        }
                                                               
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
            // Katalogdan bitkiyi bul
            var catalogPlant = await _context.Plants.FindAsync(id);
            if (catalogPlant == null)
            {
                return NotFound();
            }

            //  Mevcut kullanıcının ID'sini al
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //  UserPlant nesnesini oluştur
            var newUserPlant = new UserPlant
            {
                PlantID = catalogPlant.PlantID,
                UserID = user.UserID,
                WateringIntervalHours = catalogPlant.DefaultWateringIntervalHours,
                DateAdopted = DateTime.Now
            };

            // Sulama hesaplamasını yap
            newUserPlant.PerformWatering();

            // Veritabanına ekle
            _context.UserPlants.Add(newUserPlant);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{catalogPlant.Name} başarıyla bitkilerime eklendi!";

            return RedirectToAction("Index", "UserPlants");
        }
    }
}