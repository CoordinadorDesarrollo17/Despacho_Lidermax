using Microsoft.Data.SqlClient;
using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsRepository
    {
        Task<bool> ActualizarEstadoEntregado(TicketSeleccionadoDto model, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarEstadoEnviado(int docEntryHojaRuta, int linea, int docEntryTicket, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarEstadoHojaRuta(int docEntryHojaRuta, string estado, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarFechaDespacho(TicketsModel model);
        Task<bool> ActualizarGuiaTransportista(TicketsModel model);
        Task<bool> ActualizarObservacion(TicketSeleccionadoDto model);
        Task<bool> DevolverTicket(TicketSeleccionadoDto model, SqlConnection con, SqlTransaction tx);
        Task<bool> ExcluirTicket(TicketSeleccionadoDto model);
        Task<bool> InsertarTicketsRecogidos(int docEntryHojaRuta, int linea, int docEntryTicket, SqlConnection con, SqlTransaction tx);
        Task<IPagedList<TicketsModel>> ListadoTickets(FiltrosTicketsModel model);
        Task<List<TicketsModel>> ListadoTicketsExcel(FiltrosTicketsModel model);
        Task<IPagedList<TicketsModel>> ListadoTicketsRecogidos(FiltrosTicketsModel model);
        Task<List<TicketsModel>> ListadoTicketsRecogidosExcel(FiltrosTicketsModel model);
        Task<(int TotalTickets, int TicketsObtenidos)> ObtenerConteoTickets(int docEntryHojaRuta, string[] estado, SqlConnection con, SqlTransaction tx);
        Task<string> ObtenerEstadoTicket(int docEntryHojaRuta, int linea, int docEntryTicket, SqlConnection con, SqlTransaction tx);
    }
}
