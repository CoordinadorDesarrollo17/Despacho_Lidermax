using Microsoft.Data.SqlClient;
using Sln_Lidermax.Dtos;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsRepository
    {
        Task<bool> ActualizarEstadoEntregado(int docEntryTicket, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarEstadoEnviado(int docEntryTicket, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarEstadoHojaRuta(int docEntryHojaRuta, string estado, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarFechaDespacho(TicketsDto model);
        Task<bool> ActualizarGuiaTransportista(TicketsDto model);
        Task<bool> DevolverTicket(TicketSeleccionadoDto model, SqlConnection con, SqlTransaction tx);
        Task<bool> InsertarTicketsRecogidos(int docEntryTicket, SqlConnection con, SqlTransaction tx);
        Task<IPagedList<TicketsDto>> ListadoTickets(FiltrosTicketsDto model);
        Task<List<TicketsDto>> ListadoTicketsExcel(FiltrosTicketsDto model);
        Task<IPagedList<TicketsDto>> ListadoTicketsRecogidos(FiltrosTicketsDto model);
        Task<List<TicketsDto>> ListadoTicketsRecogidosExcel(FiltrosTicketsDto model);
        Task<(int TotalTickets, int TicketsObtenidos)> ObtenerConteoTickets(int docEntryHojaRuta, string[] estado, SqlConnection con, SqlTransaction tx);
    }
}
