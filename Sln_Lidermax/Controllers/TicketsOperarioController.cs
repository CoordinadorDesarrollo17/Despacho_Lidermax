using Azure.Core;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using Sln_Lidermax.Services;

namespace Sln_Lidermax.Controllers
{
    [Authorize(Roles = "OLID")]
    public class TicketsOperarioController : Controller
    {
        private readonly ITicketsOperarioService ticketsOperarioService;
        private readonly ITicketsService ticketsService;

        public TicketsOperarioController(ITicketsOperarioService ticketsOperarioService, ITicketsService ticketsService)
        {
            this.ticketsOperarioService = ticketsOperarioService;
            this.ticketsService = ticketsService;
        }
        public async Task<IActionResult> ListadoTicketsOperario(FiltrosTicketsModel model)
        {
            model.NombreCompleto = User.FindFirst("NombreCompleto")?.Value;

            var listaTicketsOperario = await ticketsOperarioService.ListadoTicketsOperario(model);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") //nos dice si es una peticion AJAX
            {
                return PartialView("_TablaTicketsOperario", listaTicketsOperario);
            }

            return View(listaTicketsOperario);
        }

       
        [HttpGet]
        public IActionResult ObtenerImagenes(int docNum, string tipoFiltro = null)
        {
            try
            {
                var lista = ticketsService.ObtenerImagenesLidermax(docNum, tipoFiltro);

                return Ok(new
                {
                    success = true,
                    images = lista
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EstadoEntregado([FromForm] SubirImagenesModel request)
        {
            try
            {
              
                var model = new TicketSeleccionadoDto
                {
                    DocEntryTicket = request.DocEntryTicket,
                    DocEntryHojaRuta = request.DocEntryHojaRuta,
                    DocNumTicket = request.DocNumTicket,
                    Linea = request.Linea,
                    Fecha = DateTime.Now,
                    Observacion = request.Observacion,
                    MontoFlete= request.MontoFlete,
                    IdRol =  int.Parse(User.FindFirst("IdRol")?.Value ?? "0")
                };

                bool result1 = true;
                if (!string.IsNullOrWhiteSpace(request.Transportista))
                {
                  result1 = await ticketsOperarioService.ActualizarTransportista(request.DocEntryTicket, request.Transportista);
                }

                bool result2 = true;
                if (!string.IsNullOrWhiteSpace(request.Observacion))
                {
                    result2 = await ticketsService.ActualizarObservacion(model);
                }

                var result3 = await ticketsService.SubirImagenes(request);
                var result4 = await ticketsOperarioService.RegistrarEstadoPago(request);                  
                var result5 = await ticketsService.EntregarTicket(model);


                if (!result1) return Json(new { success = false, message = "Error transportista" });
                if (!result2) return Json(new { success = false, message = "Error observacion" });
                if (!result3) return Json(new { success = false, message = "Error imágenes" });
                if (!result4) return Json(new { success = false, message = "Error pago" });
                if (!result5) return Json(new { success = false, message = "Error entrega" });

                return Json(new { success = true });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


       

    }
}
