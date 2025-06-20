using System;
using System.Security.Cryptography;
using System.Text;

namespace QuizApp.Core.Security
{
    public static class PasswordHashProvider
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100000;

        public static string GetSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            return Convert.ToBase64String(salt);
        }

        public static string GetHash(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, string salt, string hash)
        {
            string computedHash = GetHash(password, salt);
            return computedHash == hash;
        }
    }
} 