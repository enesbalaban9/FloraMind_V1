using FloraMind_V1.Models;
using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Services;
using FloraMind_V1.Data;

namespace FloraMind_V1.Services
{
    public class WateringCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public WateringCheckService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<FloraMindDbContext>();
                        var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        // Tolerans payı: 1 dakika ekleyerek kontrol ediyoruz
                        var now = DateTime.Now.AddMinutes(1);

                        var overduePlants = await _context.UserPlants
                            .Include(up => up.Plant)
                            .Include(up => up.User)
                            .Where(up => (up.IsEmailSent == false || up.IsEmailSent == null) && up.NextWateringDate <= now)
                            .ToListAsync();

                        foreach (var userPlant in overduePlants)
                        {
                            if (userPlant.User != null && userPlant.Plant != null)
                            {
                                // ÖNCELİK NICKNAME: Eğer takma ad varsa onu kullan, yoksa katalog adını kullan
                                string plantDisplayName = !string.IsNullOrEmpty(userPlant.Nickname)
                                                          ? userPlant.Nickname
                                                          : userPlant.Plant.Name;

                                // --- Mail İçeriği Düzenleme ---
                                string subject = $"{plantDisplayName} Susadı! 🌱";
                                string body = $@"Merhaba {userPlant.User.Name},

'{plantDisplayName}' isimli bitkinizin sulama vakti geldi! 💧

Bitkinizin sağlığı için lütfen en kısa sürede bakımını yapmayı unutmayın.

İyi günler dileriz,
FloraMind Ekibi";

                                try
                                {
                                    await _emailService.SendEmailAsync(userPlant.User.Email, subject, body);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine("Mail Gönderim Hatası: " + ex.Message);
                                }

                                // --- Site İçi Bildirim Kaydı ---
                                var newNotification = new Notification
                                {
                                    UserID = userPlant.UserID,
                                    Message = $"🌱 {plantDisplayName} isimli bitkinizin sulama zamanı geldi!",
                                    CreatedAt = DateTime.Now,
                                    IsRead = false
                                };
                                _context.Notifications.Add(newNotification);

                                // Gönderildi olarak işaretle (Sulama yapılınca Controller'da tekrar false yapılacak)
                                userPlant.IsEmailSent = true;
                            }
                        }

                        // Veritabanı değişikliklerini kaydet
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Servis Hatası: " + ex.Message);
                }

                // 10 saniyede bir kontrol et
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}