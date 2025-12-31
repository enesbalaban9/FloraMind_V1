namespace FloraMind_V1.Helpers
{
    public class SecurityHelper
    {
        // Şifreyi Hashleme Metodu
        public static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Şifre Doğrulama Metodu
        public static bool VerifyPassword(string enteredPassword, string storedHashPassword)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(storedHashPassword))
            {
                return false;
            }
            // Girilen şifrenin hash'i, saklanan hash ile eşleşmeli
            return HashPassword(enteredPassword) == storedHashPassword;
        }
    }
    
}
