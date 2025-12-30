using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Data;
using FloraMind_V1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace FloraMind_V1.Controllers
{
    public class PlantController : Controller
    {
        private readonly FloraMindDbContext _context;

        public PlantController(FloraMindDbContext context)
        {
            _context = context;
        }

        // GET: Plant
        public async Task<IActionResult> Index()
        {
            var plants = _context.Plants.Include(p => p.User);
            return View(await plants.ToListAsync());
        }

        // GET: Plant/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var plant = await _context.Plants
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PlantID == id);

            if (plant == null) return NotFound();

            return View(plant);
        }

        // GET: Plant/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Plant/Create
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name,Species,DefaultWateringIntervalHours")] Plant plant, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Klasör yolunu daha güvenli oluşturuyoruz
                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var extension = Path.GetExtension(imageFile.FileName);
                    var imageName = Guid.NewGuid().ToString() + extension;
                    var location = Path.Combine(directoryPath, imageName);

                    using (var stream = new FileStream(location, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    plant.ImageUrl = imageName;
                }

                plant.DateAdded = DateTime.UtcNow;
                plant.UserID = null;

                _context.Add(plant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(plant);
        }

        // GET: Plant/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var plant = await _context.Plants.FindAsync(id);
            if (plant == null) return NotFound();

            return View(plant);
        }

        // POST: Plant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlantID,Name,Species,ImageUrl,DateAdded,UserID")] Plant plant)
        {
            if (id != plant.PlantID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantExists(plant.PlantID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(plant);
        }

        // GET: Plant/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var plant = await _context.Plants
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PlantID == id);

            if (plant == null) return NotFound();

            return View(plant);
        }

        // POST: Plant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plant = await _context.Plants.FindAsync(id);
            if (plant != null)
            {
                _context.Plants.Remove(plant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Hata listesindeki CS0103 hatasının sebebi bu metodun eksik ya da yanlış yerde olmasıydı:
        private bool PlantExists(int id)
        {
            return _context.Plants.Any(e => e.PlantID == id);
        }

        // --- ÖZEL METOTLAR ---

        // 1. Bitkilerim'e Ekle
        [HttpPost]
        public async Task<IActionResult> AddToMyPlants(int id)
        {
            var originalPlant = await _context.Plants.FindAsync(id);
            if (originalPlant == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            UserPlant newUserPlant = new UserPlant
            {
                PlantID = originalPlant.PlantID,
                UserID = user.UserID,
                WateringIntervalHours = originalPlant.DefaultWateringIntervalHours,
                DateAdopted = DateTime.Now
            };

            newUserPlant.PerformWatering();

            _context.UserPlants.Add(newUserPlant);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPlants));
        }

        public async Task<IActionResult> MyPlants()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            var myPlants = await _context.UserPlants
                .Include(up => up.Plant)
                .Where(up => up.UserID == user.UserID)
                .ToListAsync();

            return View(myPlants);
        }


        [HttpPost]
        public async Task<IActionResult> WaterPlant(int id)
        {
            // Veritabanından kullanıcının bitkisini bul
            var userPlant = await _context.UserPlants.FindAsync(id);

            if (userPlant == null) return NotFound();

            // Modelde yazdığımız PerformWatering metodunu çağırarak 
            // LastWatered ve NextWateringDate alanlarını güncelliyoruz.
            userPlant.PerformWatering();

            _context.Update(userPlant);
            await _context.SaveChangesAsync();

            // İşlem bitince tekrar liste sayfasına dön
            return RedirectToAction(nameof(MyPlants));
        }
    }
}