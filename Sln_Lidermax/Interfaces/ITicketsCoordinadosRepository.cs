using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsCoordinadosRepository
    {
        Task<bool> ActualizarPlacaConductor(ConductorYPlaca model);
        Task<IPagedList<TicketsCoordinados>> ListadoTicketsCoordinadosPaginados(FiltrosTicketsModel model);
        Task<IEnumerable<string>> ObtenerConductores();
        Task<IEnumerable<string>> ObtenerPlacas();
    }
}
