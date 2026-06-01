using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsCoordinadosService
    {
        Task<IPagedList<TicketsCoordinados>> ObtenerTicketsCoordinados(FiltrosTicketsModel model);
        Task<IEnumerable<string>> ObtenerConductores();
        Task<IEnumerable<string>> ObtenerPlacas();
        Task<bool> ActualizarPlacaConductor(ConductorYPlaca model);
    }
}
