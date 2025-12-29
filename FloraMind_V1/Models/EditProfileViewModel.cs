using System.ComponentModel.DataAnnotations;

namespace FloraMind_V1.Models
{
    public class EditProfileViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; } // Yeni bilgi alanı
        public int PlantCount { get; set; }

        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; } // Şifre tekrarı
    }
}