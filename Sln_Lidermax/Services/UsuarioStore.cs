using Microsoft.AspNetCore.Identity;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;

namespace Sln_Lidermax.Services
{
    public class UsuarioStore : IUserStore<UsuarioModel>, IUserPasswordStore<UsuarioModel>
    {
        private readonly IAuthRepository authRepository;

        public UsuarioStore(IAuthRepository  authRepository)
        {
            this.authRepository = authRepository;
        }
        public Task<IdentityResult> CreateAsync(UsuarioModel user, CancellationToken cancellationToken) //modificado
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(UsuarioModel user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public Task<UsuarioModel?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<UsuarioModel?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) //modificado
        {
            return await authRepository.BuscarUsuario(normalizedUserName);
        }

        public Task<string?> GetNormalizedUserNameAsync(UsuarioModel user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetPasswordHashAsync(UsuarioModel user, CancellationToken cancellationToken)//MODIFICADO
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(UsuarioModel user, CancellationToken cancellationToken) //modificado
        {
            return Task.FromResult(user.IdUsuario.ToString());
        }

        public Task<string?> GetUserNameAsync(UsuarioModel user, CancellationToken cancellationToken) //Modificado
        {
            return Task.FromResult(user.UsuarioConcatenado);
        }

        public Task<bool> HasPasswordAsync(UsuarioModel user, CancellationToken cancellationToken) //modificado
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetNormalizedUserNameAsync(UsuarioModel user, string? normalizedName, CancellationToken cancellationToken) //modificado
        {
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(UsuarioModel user, string? passwordHash, CancellationToken cancellationToken) // modificado
        {

            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(UsuarioModel user, string? userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(UsuarioModel user, CancellationToken cancellationToken) //MODIFICADO
        {
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
