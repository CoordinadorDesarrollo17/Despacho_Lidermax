using Sln_Lidermax.Interfaces;
using BCrypt.Net;

namespace Sln_Lidermax.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration config;

        public AuthService(IConfiguration config)
        {
            this.config = config;
        }

        public bool ValidateUser(string username, string password)
        {
            var configUser = config["Auth:Username"];
            var hash = config["Auth:PasswordHash"];

            if (username != configUser)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
