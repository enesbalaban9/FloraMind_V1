using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace FloraMind_V1.Models
{
    [Authorize]
    public class EditProfileViewModel
    {
        [Required(ErrorMessage ="Ad alanı zorunludur !")]
        [MaxLength(50, ErrorMessage = "Ad alanı en fazla 50 karakter olabilir !")]
        public string Name { get; set; }
        [Required(ErrorMessage ="Email alanı zorunludur !")]
        [MaxLength(50, ErrorMessage = "Email alanı en fazla 50 karakter olabilir !")]
        public string Email { get; set; }
    }
}
