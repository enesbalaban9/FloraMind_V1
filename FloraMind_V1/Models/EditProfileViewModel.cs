using System.ComponentModel.DataAnnotations;

namespace FloraMind_V1.Models
{
    public class EditProfileViewModel
    {
        // Not: [Authorize] etiketi burada kullanılmaz, sildik.

        [Required(ErrorMessage = "Ad alanı zorunludur!")]
        [MaxLength(50, ErrorMessage = "Ad alanı en fazla 50 karakter olabilir !")]
        public string Name { get; set; } // Senin doshandaki isimle uyumlu: Name

        [Required(ErrorMessage = "Email alanı zorunludur !")]
        [MaxLength(50, ErrorMessage = "Email alanı en fazla 50 karakter olabilir !")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }

        // Controller tarafında şifre değiştirme kontrolü için bu alan şart:
        public string? Password { get; set; }
    }
}