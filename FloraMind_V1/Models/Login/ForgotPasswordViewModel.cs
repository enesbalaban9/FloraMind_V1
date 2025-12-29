using System.ComponentModel.DataAnnotations;

namespace FloraMind_V1.Models.Login
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage ="Email alanı boş bırakılamaz")]
        [EmailAddress(ErrorMessage ="geçersiz email formatı")]
        public string Email { get; set; }
    }
}
