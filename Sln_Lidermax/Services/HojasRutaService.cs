using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace Sln_Lidermax.Services
{
    public class HojasRutaService : IHojasRutaService
    {
        private readonly IHojasRutaRepository hojasRutaRepository;
        private readonly ITicketsRepository ticketsRepository;

        public HojasRutaService(IHojasRutaRepository hojasRutaRepository, ITicketsRepository ticketsRepository)
        {
            this.hojasRutaRepository = hojasRutaRepository;
            this.ticketsRepository = ticketsRepository;
        }

        public async Task<IPagedList<HojasRutaModel>> ListadoHojasRutaPaginados(FiltrosHojasRutaModel model)
        {
            return await hojasRutaRepository.ListadoHojasRutaPaginados(model);
        }

        public async Task<List<HojasRutaModel>> ListadoHojasRutaExcel(FiltrosHojasRutaModel model)
        {
            return await hojasRutaRepository.ListadoHojasRutaExcel(model);
        }

        public async Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaExcel(int docEntryHojaRuta)
        {
            var listaTickets = await hojasRutaRepository.ListadoTicketsPorHojasRutaExcel(docEntryHojaRuta);

            foreach (var ticket in listaTickets)
            {
                var detallePedido = await ticketsRepository.obtenerDet2Ticket(ticket.DocEntryTicket);
                var todasLasFacturas = new List<string>();

                foreach (var detalle in detallePedido)
                {
                    var facturas = await ticketsRepository.ObtenerFacturasxDocEntry(detalle.NroSap);
                    todasLasFacturas.AddRange(facturas);
                }
                ticket.Factura = string.Join(", ", todasLasFacturas.Distinct());
            }

            return listaTickets;
        }

        public async Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaPdf(int docEntryHojaRuta)
        {
           return  await hojasRutaRepository.ListadoTicketsPorHojasRutaPdf(docEntryHojaRuta); 
        }
    }
}
