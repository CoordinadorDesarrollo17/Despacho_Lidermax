using Microsoft.Data.SqlClient;
using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsOperarioRepository
    {
        Task<bool> ActualizarEstadoPago(SubirImagenesModel request, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarFactura(SubirImagenesModel request, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarMontoFlete(SubirImagenesModel request, SqlConnection con, SqlTransaction tx);
        Task<bool> ActualizarTransportista(int DocEntryHojaRuta, int Linea, int DocEntryTicket, string Transportista);
        Task<IPagedList<TicketsModel>> ListadoTicketsOperario(FiltrosTicketsModel model);
    }
}
