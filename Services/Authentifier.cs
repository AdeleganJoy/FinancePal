using System.Security.Cryptography;
using System.Text;

namespace FinancePal.Services
{
    public class Authentifier
    {
        private const string Pepper = "Chdakhdsofsjpj'psASO"; 
        public string HashPassword(string password, out string salt)
        {
            salt = GenerateSalt();
            string salt_password_pepper = password + salt + Pepper;

            using (var sha256 = SHA256.Create()){
                var hash_password = sha256.ComputeHash(Encoding.UTF8.GetBytes(salt_password_pepper));
                return Convert.ToBase64String(hash_password);
            }
        }
        private string GenerateSalt()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[16]; 
                rng.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
        }
        public bool ValidatePassword(string input_password, string correct_hash, string correct_salt){
            string combinedPassword = input_password + correct_salt + Pepper;
            using (var sha256 = SHA256.Create()){
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedPassword));
                string enteredHash = Convert.ToBase64String(hashBytes);

                return correct_hash == enteredHash;
            }
        }
            }
    }
