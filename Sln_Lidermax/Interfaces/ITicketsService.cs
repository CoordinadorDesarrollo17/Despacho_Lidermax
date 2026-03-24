using Sln_Lidermax.Dtos;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsService
    {
        Task<bool> ActualizarFechaDespacho(TicketsDto model);
        Task<bool> ActualizarGuiaTransportista(TicketsDto model);
        Task<bool> DevolverTicket(TicketSeleccionadoDto model);
        Task<bool> EntregarTicket(TicketSeleccionadoDto request);
        Task<bool> InsertarTicketsRecogidos(RecogerTicketsDto request);
        Task<IPagedList<TicketsDto>> ListadoTickets(FiltrosTicketsDto model);
        Task<List<TicketsDto>> ListadoTicketsExcel(FiltrosTicketsDto model);
        Task<IPagedList<TicketsDto>> ListadoTicketsRecogidos(FiltrosTicketsDto model);
        Task<List<TicketsDto>> ListadoTicketsRecogidosExcel(FiltrosTicketsDto model);
        List<object> ObtenerImagenesLidermax(int docNumTicket);
        Task<bool> SubirImagenes(SubirImagenesDto request);
    }
}
