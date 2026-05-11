using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
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

        public async Task<IPagedList<HojasRutaModel>> ListadoHojasRutaPaginados(FiltrosHojasRutaModel model)
        {
            return await ticketsRepository.ListadoHojasRutaPaginados(model);
        }

        public async Task<List<HojasRutaModel>> ListadoHojasRutaExcel(FiltrosHojasRutaModel model)
        {
            return await ticketsRepository.ListadoHojasRutaExcel(model);
        }

        public async Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaExcel(int docEntryHojaRuta)
        {
            return await ticketsRepository.ListadoTicketsPorHojasRutaExcel(docEntryHojaRuta);
        }

        public async Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaPdf(int docEntryHojaRuta)
        {
            return await ticketsRepository.ListadoTicketsPorHojasRutaPdf(docEntryHojaRuta);
        }
    }
}
