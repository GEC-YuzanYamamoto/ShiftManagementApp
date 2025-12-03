using System.Security.Cryptography;
using System.Text;

namespace ShiftApi.ApiService.Services
{
    public class PasswordHasher
    {
        public static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);

            return Convert.ToHexString(hash);
        }
    }
}
