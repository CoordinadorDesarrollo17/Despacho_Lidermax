using Microsoft.AspNetCore.Mvc;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;

namespace Sln_Lidermax.Controllers
{
    public class TicketsCoordinadosController : Controller
    {
        private readonly ITicketsCoordinadosService ticketsCoordinadosService;
        private readonly ITicketsService ticketsService;

        public TicketsCoordinadosController(ITicketsCoordinadosService ticketsCoordinadosService, ITicketsService ticketsService)
        {
            this.ticketsCoordinadosService = ticketsCoordinadosService;
            this.ticketsService = ticketsService;
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

        [HttpGet]
        public IActionResult DescargarArchivo(string docNum)
        {
            if (string.IsNullOrWhiteSpace(docNum))
                return BadRequest();

            // Evitar caracteres inválidos en el nombre de archivo
            if (docNum.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return BadRequest();

            string ruta = Path.Combine("\\\\192.168.1.40\\", "TicketsEditarPDF");
            string fileName = $"{docNum}.pdf";
            string fullPath = Path.Combine(ruta, fileName).Replace("\\", "/");

            bool exists = System.IO.File.Exists(fullPath);

            // Si la petición es AJAX (verificación), devolvemos JSON con existencia
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { exists });
            }

            // Si no es AJAX, intentamos servir el archivo (navegación/direct download)
            if (!exists)
                return NotFound();

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, "application/pdf", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> EstadoEntregado([FromBody] TicketSeleccionadoDto model)
        {
            try
            {
                model.Fecha = DateTime.Now;

                var result = await ticketsService.EntregarTicket(model);

                if (!result) return Json(new { success = false, message = "Error actualizando estado a entregado" });

                return Json(new
                {
                    success = true,
                    message = "Ticket actualizado correctamente."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
