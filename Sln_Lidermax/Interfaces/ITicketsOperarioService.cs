using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsOperarioService
    {
        Task<bool> ActualizarTransportista(TicketsModel model);
        Task<IPagedList<TicketsModel>> ListadoTicketsOperario(FiltrosTicketsModel model);
        Task<bool> RegistrarEstadoPago(SubirImagenesModel request);
    }
}
