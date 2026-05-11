using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface IHojasRutaRepository
    {
        Task<List<HojasRutaModel>> ListadoHojasRutaExcel(FiltrosHojasRutaModel model);
        Task<IPagedList<HojasRutaModel>> ListadoHojasRutaPaginados(FiltrosHojasRutaModel model);
        Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaExcel(int docEntryHojaRuta);
        Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaPdf(int docEntryHojaRuta);
    }
}
