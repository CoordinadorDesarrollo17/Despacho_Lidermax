using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface IHojasRutaService
    {
        Task<List<HojasRutaModel>> ListadoHojasRutaExcel(FiltrosHojasRutaModel model);
        Task<IPagedList<HojasRutaModel>> ListadoHojasRutaPaginados(FiltrosHojasRutaModel model);
        Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaExcel(int docEntryHojaRuta);
        Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaPdf(int docEntryHojaRuta);
        Task<DireccionProvinciaSuelta_E> ObtenerDireccionProvinciaSuelta(string numAtCard);
    }
}
