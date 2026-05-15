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

        [HttpPost]
        public async Task<IActionResult> ActualizarTransportista([FromBody] TicketsModel model)
        {
            try
            {
                var result = ticketsOperarioService.ActualizarTransportista(model);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
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
        public async Task<IActionResult> SubirImagenes([FromForm] SubirImagenesModel request)
        {
            try
            {
                var result = await ticketsService.SubirImagenes(request);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> RegistrarPago([FromForm] SubirImagenesModel request)
        {
            try
            {
                var result = await ticketsOperarioService.RegistrarEstadoPago(request); 

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EstadoEntregado([FromBody] TicketSeleccionadoDto model)
        {
            try
            {
                model.Fecha = DateTime.Now;
                var result = await ticketsService.EntregarTicket(model);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
