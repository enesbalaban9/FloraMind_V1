using Microsoft.EntityFrameworkCore;
using FloraMind_V1.Models;
using FloraMind_V1.Models.Login;

namespace FloraMind_V1.Data
{

    
    public class FloraMindDbContext : DbContext    //veritabanı ile site arasındaki köprü sınıf
    {
        public FloraMindDbContext(DbContextOptions<FloraMindDbContext> options) // Yapıcı metod
            : base(options){}
        public DbSet<User> Users { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<UserPlant> UserPlants { get; set; }
        public DbSet<ForgotPassword> ForgottenPasswords { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder) // Veritabanı ilişkilerini tanımlama
        {
            base.OnModelCreating(modelBuilder);

            
            // User-Plant İlişkisi
            
            modelBuilder.Entity<Plant>()
                .HasOne(p => p.User)
                .WithMany(u => u.Plants)
                .HasForeignKey(p => p.UserID)
                .IsRequired(false)             // Katalog bitkisi kimseye ait olmayabilir (NULL)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinse bile katalog bitkisi kalır

            
            // User-Content) İlişkisi

            modelBuilder.Entity<Content>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contents)
                .HasForeignKey(c => c.UserID)
                .IsRequired(true)             // Veritabanı seviyesinde zorunlu kılındı (NOT NULL)
                .OnDelete(DeleteBehavior.Restrict); // Admin silinse bile içerikler korunur

            
            //Plant-Content İlişkisi
            
            modelBuilder.Entity<Content>()
                .HasOne(c => c.Plant)
                .WithMany(p => p.Contents)
                .HasForeignKey(c => c.PlantID)
                .IsRequired(true)             // İçeriğin mutlaka bir bitkiye ait olması zorunlu kılındı
                .OnDelete(DeleteBehavior.Cascade); // Bitki silinirse, ona ait tüm içerikler de silinir

           
            //User-UserPlant
            
            modelBuilder.Entity<UserPlant>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPlants)
                .HasForeignKey(up => up.UserID)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinirse koleksiyon kayıtları silinir

            modelBuilder.Entity<UserPlant>()
        .HasOne(up => up.Plant)          // Her UserPlant'in bir Plant'i vardır
        .WithMany(p => p.UserPlants)     // Her Plant'in birden fazla UserPlant kaydı olabilir
        .HasForeignKey(up => up.PlantID) // Yabancı anahtar PlantId sütunudur
        .OnDelete(DeleteBehavior.Cascade);


            //Plant-UserPlant İlişkisi

            modelBuilder.Entity<UserPlant>()
                .HasOne(up => up.Plant)
                
                .WithMany(p => p.UserPlants) 
                .HasForeignKey(up => up.PlantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>(entity =>
            {
                // Varsayılan Değer Atama
                entity.Property(e => e.Role)
                      .HasDefaultValue("User");

                entity.Property(e => e.Role)
                      .HasMaxLength(50);

                entity.Property(e => e.Role)
                      .IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                // RegistrationDate için varsayılan değeri SQL server'ın atamasını sağlar
                
                entity.Property(e => e.RegistrationDate)
                      .HasDefaultValueSql("GETDATE()"); 
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.LastLoginDate)
                      .IsRequired(false); 
            });

            modelBuilder.Entity<User>(entity =>
            {
                
                entity.Property(e => e.IsBanned)
                      .HasDefaultValue(false); 
            });
        }
    }
}

