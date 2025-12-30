using FloraMind_V1.Models;
using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Services;

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
                        var _context = scope.ServiceProvider.GetRequiredService<FloraMind_V1.Data.FloraMindDbContext>();
                        var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        var now = DateTime.Now;

                        var overduePlants = await _context.UserPlants
                            .Include(up => up.Plant)
                            .Include(up => up.User)
                            .Where(up => up.NextWateringDate <= now && up.IsEmailSent == false)
                            .ToListAsync();

                        foreach (var userPlant in overduePlants)
                        {
                            if (userPlant.User != null)
                            {
                                string plantName = userPlant.Nickname ?? userPlant.Plant.Name;

                                // 1. ADIM: E-POSTA GÖNDERİMİ
                                if (!string.IsNullOrEmpty(userPlant.User.Email))
                                {
                                    string subject = "Bitkinizin Su Zamanı Geldi! 🌱";
                                    string body = $@"
                                        <h3>Merhaba {userPlant.User.Name},</h3>
                                        <p><b>{plantName}</b> isimli bitkinin sulama zamanı geldi.</p>
                                        <p>Onu susuz bırakmamak için en kısa sürede sulamayı unutma!</p>
                                        <br>
                                        <p>FloraMind Ekibi</p>";

                                    try
                                    {
                                        await _emailService.SendEmailAsync(userPlant.User.Email, subject, body);
                                    }
                                    catch (Exception emailEx)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Mail Hatası: " + emailEx.Message);
                                    }
                                }

                                // 2. ADIM: SİTE İÇİ BİLDİRİM OLUŞTURMA
                                var newNotification = new Notification
                                {
                                    UserID = userPlant.UserID,
                                    Message = $"🌱 {plantName} isimli bitkinizin sulama zamanı geldi!",
                                    CreatedAt = DateTime.Now,
                                    IsRead = false
                                };
                                _context.Notifications.Add(newNotification);

                                // Mail ve Bildirim işlemleri bittiği için bayrağı işaretle
                                userPlant.IsEmailSent = true;
                            }
                        }

                        if (overduePlants.Any())
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Bildirim Servisi Genel Hatası: " + ex.Message);
                }

                // Sunum için 5 saniyede bir kontrol eder
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}