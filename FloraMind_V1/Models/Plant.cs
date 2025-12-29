using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // [NotMapped] için gerekli
using Microsoft.AspNetCore.Http; // IFormFile için bu kütüphane şart!
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FloraMind_V1.Models
{
    public class Plant  // Bitki Modeli
    {
        [Key]
        public int PlantID { get; set; } // Bitki ID'si

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Bitki Adı

        [MaxLength(100)]
        public string Species { get; set; } // Bitki Türü

        public DateTime DateAdded { get; set; } = DateTime.UtcNow; // Bitkinin Eklenme tarihi (Otomatik)

        // --- YENİ EKLENEN KISIMLAR (FOTOĞRAF İÇİN) ---

        // 1. Veritabanına resmin dosya yolu kaydedilecek (Örn: \images\plants\resim.jpg)
        [ValidateNever]
        public string? ImageUrl { get; set; }

        // 2. Formdan gelen dosya burada tutulacak. 
        // [NotMapped] veritabanında bu isimde bir kolon oluşturulmasını engeller.
        [NotMapped]
        [ValidateNever]
        public IFormFile? ImageUpload { get; set; }

        // ----------------------------------------------

        // İlişkiler
        [ValidateNever]
        public User User { get; set; } // Bitki Sahibi Kullanıcı Nesnesi

        public int? UserID { get; set; }

        [ValidateNever]
        public ICollection<Content> Contents { get; set; } // Bitkiye Ait İçerikler

        [ValidateNever]
        public ICollection<UserPlant> UserPlants { get; set; } // Kullanıcı-Bitki İlişkisi
    }
}
