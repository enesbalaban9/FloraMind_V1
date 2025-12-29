using System.ComponentModel.DataAnnotations;

namespace FloraMind_V1.Models.Login
{
    ///<summary>
    /// Şifremi unuttum sayfasından gelen e posta bilgisini taşımak ve
    /// kullanıcı girdisini doğrulamak için kullanılır.
    /// </summary>
    public class ForgotPassword
    {
            public int Id { get; set; }

            [Required]
        
            public string Email { get; set; }

            public string? VerificationCode { get; set; }

            public DateTime? ExpirationDate { get; set; }

            public bool IsUsed { get; set; } = false;

            //foreign key

            public int UserID { get; set; }
            public User User { get; set; }

    }
}
