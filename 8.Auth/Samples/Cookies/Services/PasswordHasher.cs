using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Cookies.Services
{
    public interface IPasswordHasher
    {
        (string hash, string salt) HashPassword(string password);
        bool VerifyPassword(string password, string hash, string salt);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public (string hash, string salt) HashPassword(string password)
        {
            // crea un 184-bit salt usando lo que nos recomendo usar
            byte[] saltBytes = RandomNumberGenerator.GetBytes(128 / 8);
            string salt = Convert.ToBase64String(saltBytes);

            // No se lo que hace pero gemini lo recomendo 
            string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return (hash, salt);
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);

            string hashToVerify = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hash == hashToVerify;
        }
    }
}
