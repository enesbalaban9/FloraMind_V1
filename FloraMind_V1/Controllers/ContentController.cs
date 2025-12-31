using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Data;
using FloraMind_V1.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FloraMind_V1.Controllers
{
    
    public class ContentController : Controller
    {
        private readonly FloraMindDbContext _context;

        public ContentController(FloraMindDbContext context)
        {
            _context = context;
        }

        // Giriş yapan kullanıcının ID'sini alır
        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            
            throw new UnauthorizedAccessException("Oturum açmış kullanıcı ID'si bulunamadı veya geçerli değil.");
        }


        // GET: Content
        public async Task<IActionResult> Index()
        {
           
            var contents = await _context.Contents
                .Include(c => c.Plant)
                .Include(c => c.User)
                .ToListAsync();

            return View(contents);
        }

        // GET: Content/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var content = await _context.Contents
                .Include(c => c.Plant)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.ContentID == id);

            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        // GET: Content/Create
        public async Task<IActionResult> Create()
        {
            
            ViewBag.PlantID = new SelectList(await _context.Plants.ToListAsync(), "PlantID", "Name");
            return View();
        }

        // POST: Content/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Body,PlantID")] Content content)
        {

            content.UserID = GetLoggedInUserId(); 

            ModelState.Remove("UserID");
            ModelState.Remove("User"); 

            
            if (content.PlantID <= 0)
            {
                ModelState.AddModelError("PlantID", "Lütfen bir bitki seçiniz.");
            }
            else
            {
                
                ModelState.Remove("PlantID");
                ModelState.Remove("Plant");
            }

            
            if (ModelState.IsValid)
            {
                content.DateCreated = DateTime.UtcNow;
                _context.Contents.Add(content);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa dropdown'ı tekrar doldur
            ViewBag.PlantID = new SelectList(await _context.Plants.ToListAsync(), "PlantID", "Name", content.PlantID);
            return View(content);
        }


        // GET: Content/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var content = await _context.Contents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }
            // Sadece Plant listesi View'a gönderilir
            ViewData["PlantID"] = new SelectList(await _context.Plants.ToListAsync(), "PlantID", "Name", content.PlantID);
            return View(content);
        }

        // POST: Content/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContentID,Title,Body,PlantID,UserID,DateCreated")] Content content)
        {
            if (id != content.ContentID)
            {
                return NotFound();
            }

            // UserID ve PlantID hatalarını manuel olarak temizlenmesi
            ModelState.Remove("UserID");
            ModelState.Remove("User");
            ModelState.Remove("PlantID");
            ModelState.Remove("Plant");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(content); 
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContentExists(content.ContentID))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Hata olursa dropdown tekrar dolsun
            ViewBag.PlantID = new SelectList(await _context.Plants.ToListAsync(), "PlantID", "Name", content.PlantID);
            return View(content);
        }

        // GET: Content/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Details ile aynı mantık: ilişkili verileri yükle
            if (id == null)
            {
                return NotFound();
            }

            var content = await _context.Contents
                .Include(c => c.Plant)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.ContentID == id);

            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        // POST: Content/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content != null)
            {
                _context.Contents.Remove(content);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContentExists(int id)
        {
            return _context.Contents.Any(e => e.ContentID == id);
        }
    }
}