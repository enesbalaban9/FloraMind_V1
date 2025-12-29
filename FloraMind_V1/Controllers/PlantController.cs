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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Species")] Plant plant, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Resim Yükleme İşlemi
                if (imageFile != null)
                {
                    var extension = Path.GetExtension(imageFile.FileName);
                    var imageName = Guid.NewGuid() + extension;
                    var location = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/", imageName);

                    var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (var stream = new FileStream(location, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    plant.ImageUrl = imageName;
                }

                plant.DateAdded = DateTime.UtcNow;
                plant.UserID = null; // Login sistemi gelene kadar null

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
        public async Task<IActionResult> AddToMyPlants(int id)
        {
            var originalPlant = await _context.Plants.FindAsync(id);
            if (originalPlant == null) return NotFound();

            Plant newUserPlant = new Plant
            {
                Name = originalPlant.Name,
                Species = originalPlant.Species,
                ImageUrl = originalPlant.ImageUrl,
                DateAdded = DateTime.UtcNow,
                UserID = null
            };

            _context.Add(newUserPlant);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyPlants));
        }

        // 2. Bitkilerim Sayfası
        public async Task<IActionResult> MyPlants()
        {
            var myPlants = _context.Plants
                .Where(p => p.UserID == null);

            return View(await myPlants.ToListAsync());
        }
    }
}