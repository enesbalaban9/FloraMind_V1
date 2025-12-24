using FloraMind_V1.Data;
using FloraMind_V1.Models;
using Microsoft.EntityFrameworkCore;
namespace FloraMind_V1.Services
{
    
    public class UserService : IUserService
    {
        private readonly FloraMindDbContext _context;

        public UserService(FloraMindDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
          
            return await _context.Users.ToListAsync();
        }


        // Rol Güncelleme İşlemi 
        public async Task UpdateUserRole(int userId, string NewRole)
        {
            //  Kullanıcıyı bul
            var userToUpdate = await _context.Users.FindAsync(userId);

            if (userToUpdate == null)
            {
                // Kullanıcı bulunamazsa hata fırlat
                throw new InvalidOperationException($"Kullanıcı ID: {userId} bulunamadı.");
            }

            // Rol özelliğini değiştir
            userToUpdate.Role = NewRole;

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLastLoginDateAsync(int userId)
        {
            // Kullanıcıyı tracking açık olacak şekilde bulma
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                // LastLoginDate alanını o anki UTC zamanı ile güncelleme
                user.LastLoginDate = DateTime.UtcNow;

                // Değişiklikleri veritabanına kalıcı olarak kaydetme
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetUsersAsync(string searchString = null)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var term = searchString.Trim();

                users = users.Where(u =>
                    EF.Functions.Like(u.Name, $"%{term}%")
                );

                // Eğer sadece rakamsa ID'den ara
                if (int.TryParse(term, out int userId))
                {
                    users = users.Union(
                        _context.Users.Where(u => u.UserID == userId)
                    );
                }
            }

            return await users.ToListAsync();
        }




        public async Task ToggleBanStatusAsync(int userId, bool isBanned)
        {
            // Kullanıcıyı ID ile bulma
            var userToUpdate = await _context.Users.FindAsync(userId);

            if (userToUpdate != null)
            {
                // IsBanned durumunu güncelle
                userToUpdate.IsBanned = isBanned;

                // Değişiklikleri veritabanına kaydet
                await _context.SaveChangesAsync();
            }
            
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {

            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        }
    }
}
