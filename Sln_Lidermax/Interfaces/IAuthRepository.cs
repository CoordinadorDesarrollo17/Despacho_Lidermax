using Sln_Lidermax.Models;

namespace Sln_Lidermax.Interfaces
{
    public interface IAuthRepository
    {
        Task<UsuarioModel> BuscarUsuario(string usuario);
    }
}
