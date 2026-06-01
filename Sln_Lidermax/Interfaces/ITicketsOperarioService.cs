using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface ITicketsOperarioService
    {
        Task<bool> ActualizarTransportista(int DocEntryHojaRuta, int Linea, int DocEntryTicket, string Transportista);
        Task<IPagedList<TicketsModel>> ListadoTicketsOperario(FiltrosTicketsModel model);
        Task<bool> RegistrarEstadoPago(SubirImagenesModel request);
    }
}
