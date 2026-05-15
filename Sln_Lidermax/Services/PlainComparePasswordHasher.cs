using Microsoft.AspNetCore.Identity;
using Sln_Lidermax.Models;
using System.Security.Cryptography;
using System.Text;

namespace Sln_Lidermax.Services
{

    public class PlainComparePasswordHasher : IPasswordHasher<UsuarioModel>
    {
        public string HashPassword(UsuarioModel user, string password)
        {
            return password;
        }

        public PasswordVerificationResult VerifyHashedPassword(UsuarioModel user,string hashedPassword,string providedPassword)
        {
            // hashedPassword aquí es el password descifrado (PasswordPlano)
            if (hashedPassword == null || providedPassword == null)
            {
                return PasswordVerificationResult.Failed;
            }
            
            // Comparación en tiempo constante para evitar timing attacks
            var a = Encoding.UTF8.GetBytes(hashedPassword);
            var b = Encoding.UTF8.GetBytes(providedPassword);

            if (a.Length != b.Length)
            {
                return PasswordVerificationResult.Failed;
            }       

            return CryptographicOperations.FixedTimeEquals(a, b)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }

}
