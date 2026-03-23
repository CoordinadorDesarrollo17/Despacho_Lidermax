using Sln_Lidermax.Dtos;
using X.PagedList;

namespace Sln_Lidermax.Interfaces
{
    public interface IHojasRutaRepository
    {
        Task<List<HojasRutaDto>> ListadoHojasRutaExcel(FiltrosHojasRutaDto model);
        Task<IPagedList<HojasRutaDto>> ListadoHojasRutaPaginados(FiltrosHojasRutaDto model);
    }
}
