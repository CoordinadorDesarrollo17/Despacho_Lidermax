using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Services
{
    public class TicketsCoordinadosService : ITicketsCoordinadosService
    {
        private readonly ITicketsCoordinadosRepository ticketsCoordinadosRepository;

        public TicketsCoordinadosService(ITicketsCoordinadosRepository TicketsCoordinadosRepository)
        {
            ticketsCoordinadosRepository = TicketsCoordinadosRepository;
        }

        public async Task<IPagedList<TicketsCoordinados>> ObtenerTicketsCoordinados(FiltrosTicketsModel model)
        {
            return await ticketsCoordinadosRepository.ListadoTicketsCoordinadosPaginados(model);
        }
        public async Task<IEnumerable<string>> ObtenerConductores()
        {
            return await ticketsCoordinadosRepository.ObtenerConductores();
        }

        public async Task<IEnumerable<string>> ObtenerPlacas()
        {
            return await ticketsCoordinadosRepository.ObtenerPlacas();
        }

        public async Task<bool> ActualizarPlacaConductor(ConductorYPlaca model)
        {
            return await ticketsCoordinadosRepository.ActualizarPlacaConductor(model);
        }
    }
}
