using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsService
    {
        Task<bool> ActualizarFechaDespacho(TicketsModel model);
        Task<bool> ActualizarGuiaTransportista(TicketsModel model);
        Task<bool> ActualizarObservacion(TicketsModel model);
        Task<bool> DevolverTicket(TicketSeleccionadoDto model);
        Task<bool> EntregarTicket(TicketSeleccionadoDto request);
        Task<bool> InsertarTicketsRecogidos(RecogerTicketsModel request);
        Task<IPagedList<TicketsModel>> ListadoTickets(FiltrosTicketsModel model);
        Task<List<TicketsModel>> ListadoTicketsExcel(FiltrosTicketsModel model);
        Task<IPagedList<TicketsModel>> ListadoTicketsRecogidos(FiltrosTicketsModel model);
        Task<List<TicketsModel>> ListadoTicketsRecogidosExcel(FiltrosTicketsModel model);
        List<object> ObtenerImagenesLidermax(int docNumTicket);
        Task<bool> SubirImagenes(SubirImagenesModel request);
    }
}
