using System.Security.Cryptography;
using System.Text;

namespace WebAppComp3011.Services
{
    public class PasswordHashingService
    {
        /// <summary>
        /// Hash a password using SHA256 with a salt
        /// </summary>
        public static string HashPassword(string password, string username = null, bool isDevelopment = false)
        {
            if (string.IsNullOrWhiteSpace(password))
                return string.Empty;

            // If in development mode and username is abc, def, or xyz, skip hashing
            if (isDevelopment && username != null)
            {
                var devUsers = new[] { "abc", "def", "xyz" };
                if (devUsers.Contains(username.ToLowerInvariant()))
                    return password;
            }

            // Generate a random salt (16 bytes)
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash password with salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            // Combine salt and hash
            byte[] hashWithSalt = new byte[36];
            Array.Copy(salt, 0, hashWithSalt, 0, 16);
            Array.Copy(hash, 0, hashWithSalt, 16, 20);

            // Convert to base64 string
            return Convert.ToBase64String(hashWithSalt);
        }

        /// <summary>
        /// Verify a password against a stored hash
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            try
            {
                // Get the bytes from the stored hash
                byte[] hashWithSalt = Convert.FromBase64String(storedHash);

                // Extract the salt (first 16 bytes)
                byte[] salt = new byte[16];
                Array.Copy(hashWithSalt, 0, salt, 0, 16);

                // Hash the incoming password with the extracted salt
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(20);

                // Compare the hash (bytes 16-36)
                for (int i = 0; i < 20; i++)
                {
                    if (hashWithSalt[i + 16] != hash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
