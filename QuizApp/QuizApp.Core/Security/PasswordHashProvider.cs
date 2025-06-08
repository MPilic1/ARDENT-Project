using System;
using System.Security.Cryptography;
using System.Text;

namespace QuizApp.Core.Security
{
    public static class PasswordHashProvider
    {
        public static string GetSalt()
        {
            byte[] salt = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(salt);
        }

        public static string GetHash(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using (var hmac = new HMACSHA256(saltBytes))
            {
                byte[] hash = hmac.ComputeHash(passwordBytes);
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