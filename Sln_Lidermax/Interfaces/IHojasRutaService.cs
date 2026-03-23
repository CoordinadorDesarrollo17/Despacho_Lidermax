using Sln_Lidermax.Dtos;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface IHojasRutaService
    {
        Task<List<HojasRutaDto>> ListadoHojasRutaExcel(FiltrosHojasRutaDto model);
        Task<IPagedList<HojasRutaDto>> ListadoHojasRutaPaginados(FiltrosHojasRutaDto model);
       
    }
}
