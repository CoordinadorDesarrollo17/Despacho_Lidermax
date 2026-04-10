using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Dtos;
using X.PagedList;

namespace Sln_Lidermax.Services
{
    public class HojasRutaService : IHojasRutaService
    {
        private readonly IHojasRutaRepository ticketsRepository;

        public HojasRutaService(IHojasRutaRepository ticketsRepository)
        {
            this.ticketsRepository = ticketsRepository;
        }

        public async Task<IPagedList<HojasRutaDto>> ListadoHojasRutaPaginados(FiltrosHojasRutaDto model)
        {
            return await ticketsRepository.ListadoHojasRutaPaginados(model);
        }

        public async Task<List<HojasRutaDto>> ListadoHojasRutaExcel(FiltrosHojasRutaDto model)
        {
            return await ticketsRepository.ListadoHojasRutaExcel(model);
        }

        public async Task<List<ExcelHojaRutaDto>> ListadoTicketsPorHojasRutaExcel(int docEntryHojaRuta)
        {
            return await ticketsRepository.ListadoTicketsPorHojasRutaExcel(docEntryHojaRuta);
        }

    }
}
