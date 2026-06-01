using Microsoft.AspNetCore.Mvc;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Controllers
{
    public class TicketsCoordinadosController : Controller
    {
        private readonly ITicketsCoordinadosService ticketsCoordinadosService;

        public TicketsCoordinadosController(ITicketsCoordinadosService ticketsCoordinadosService )
        {
            this.ticketsCoordinadosService = ticketsCoordinadosService;
        }
        public async Task<IActionResult> ListadoTicketsCoordinados(FiltrosTicketsModel model)
        {
            var lista = await ticketsCoordinadosService.ObtenerTicketsCoordinados(model);
           
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") //nos dice si es una peticion AJAX
            {
                return PartialView("_TablaTicketsCoordinados", lista);
            }

            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerConductores()
        {
            var lista = await ticketsCoordinadosService.ObtenerConductores();
            return Json(lista);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPlacas()
        {
            var lista = await ticketsCoordinadosService.ObtenerPlacas();
            return Json(lista);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarPlacaConductor([FromBody] ConductorYPlaca model)
        {

            bool resultado = await ticketsCoordinadosService.ActualizarPlacaConductor(model);

            if (resultado)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "No se encontró el registro." });
            }
               
        }
    }
}
