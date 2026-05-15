using Sln_Lidermax.Interfaces;
using BCrypt.Net;
using Sln_Lidermax.Models;

namespace Sln_Lidermax.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration config;
        private readonly IAuthRepository authRepository;

        public AuthService(IConfiguration config, IAuthRepository authRepository)
        {
            this.config = config;
            this.authRepository = authRepository;
        }      

        public async Task<UsuarioModel> BuscarUsuario(string usuario)
        {
            return await authRepository.BuscarUsuario(usuario);
        }
    }
}
