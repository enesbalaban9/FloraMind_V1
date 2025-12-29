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
            var plants = _context.Plants.ToList(); 
            return View(plants);
        }
        public async Task <IActionResult> UserProfileDetails(int id)
        {

            var UserDetails = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == id);
            return View();

        }

        public async Task<IActionResult> ChangeUserName()
        {
            
            return View();
        }
    }
}
