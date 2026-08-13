using Azure.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sln_Lidermax.Models;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Services;
using System.Reflection;

namespace Sln_Lidermax.Controllers
{
    [Authorize(Roles = "MANAGER,SLID")]
    public class TicketsController : Controller
    {
        private readonly ITicketsService ticketsService;

        public TicketsController(ITicketsService ticketsService ) {
            this.ticketsService = ticketsService;
        }
        public async Task<IActionResult> ListadoTickets(FiltrosTicketsModel model)
        {
            ViewBag.DocEntryHojaRuta = model.DocEntryHojaRuta;
            ViewBag.DocNumHojaRuta = model.DocNumHojaRuta;
            var listaTickets = await ticketsService.ListadoTickets(model);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") //nos dice si es una peticion AJAX
            {
                return PartialView("_TablaTickets", listaTickets);
            }
            return View(listaTickets);
        }

        [HttpPost]
        public async Task<IActionResult> RecogerTickets([FromBody] RecogerTicketsModel request)
        {
            if (request.Tickets == null || !request.Tickets.Any())
            {
                return Json(new { success = false, mensaje = "No se enviaron tickets" });
            }

            try
            {
                var result = await ticketsService.InsertarTicketsRecogidos(request);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarFechaDespacho([FromBody] TicketsModel model)
        {
            try
            {
                var result = await ticketsService.ActualizarFechaDespacho(model);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarGuiaTransportista([FromBody] TicketsModel model)
        {
            try
            {
                var result = await ticketsService.ActualizarGuiaTransportista(model);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarObservacion([FromBody] TicketSeleccionadoDto model)
        {
            try
            {
                var result = await ticketsService.ActualizarObservacion(model);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DetalleTicket(int id)
        {
            var model = new FiltrosTicketsModel
            {
                DocEntryTicket = id
            };

            var lista = await ticketsService.ListadoTicketsRecogidos(model);

            var ticket = lista.FirstOrDefault(); 

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        public async Task<IActionResult> ListadoTicketsRecogidos(FiltrosTicketsModel model)
        {   
            var listaTicketsRecogidos = await ticketsService.ListadoTicketsRecogidos(model);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") //nos dice si es una peticion AJAX
            {
                return PartialView("_TablaTicketsRecogidos", listaTicketsRecogidos);
            }

            return View(listaTicketsRecogidos);
        }

        [HttpPost]
        public async Task<IActionResult> DevolverTicket([FromBody] TicketSeleccionadoDto model)
        {
            try
            {
                var result = await ticketsService.DevolverTicket(model);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> EntregarTicket([FromBody] TicketSeleccionadoDto model)
        {
            try
            {
                var result = await ticketsService.EntregarTicket(model);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
        public async Task<IActionResult> ActualizarDatos([FromForm] SubirImagenesModel request)
        {
            try
            {
                var result = await ticketsService.ActualizarDatos(request);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> ExportarExcelTicketsHojaRuta(FiltrosTicketsModel model)
        {
            IEnumerable<TicketsModel> lista;

            if (model.esHojaRuta == true)
            {
                lista = await ticketsService.ListadoTicketsExcel(model);
            }
            else
            {
                lista = await ticketsService.ListadoTicketsRecogidosExcel(model);
            }

            string NameHoja = model.esHojaRuta == true ? $"Tickets Hoja Ruta" : $"Tickets Recogidos";

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(NameHoja);

            // CABECERAS
            ws.Cell(1, 1).Value = "Nro ticket";
            ws.Cell(1, 2).Value = "Guia Remisión";
            ws.Cell(1, 3).Value = "Guia Transportista";
            ws.Cell(1, 4).Value = "Cod. Cliente";
            ws.Cell(1, 5).Value = "Razón Social";
            ws.Cell(1, 6).Value = "Dirección Envio";
            ws.Cell(1, 7).Value = "Destino";
            ws.Cell(1, 8).Value = "Empresa Transporte";
            ws.Cell(1, 9).Value = "Distrito Transporte";
            ws.Cell(1, 10).Value = "Modo Envio";
            ws.Cell(1, 11).Value = "Cajas";
            ws.Cell(1, 12).Value = "Peso";
            ws.Cell(1, 13).Value = "Monto Flete";
            ws.Cell(1, 14).Value = "Contacto";
            ws.Cell(1, 15).Value = "Telefono";
            ws.Cell(1, 16).Value = "Estado";
            ws.Cell(1, 17).Value = "Observacion";
       
            int colCount = 17;

            // Si no es hoja de ruta, agregamos dos columnas extra
            if (model.esHojaRuta == false)
            {
                ws.Cell(1, 18).Value = "Placa";
                ws.Cell(1, 19).Value = "Factura";
                ws.Cell(1, 20).Value = "Fecha Recojo";
                ws.Cell(1, 21).Value = "Hora Pactada";
                ws.Cell(1, 22).Value = "Fecha Entrega";
                colCount = 22;
            }

            ws.Range(1, 1, 1, colCount).Style.Font.Bold = true;

            int row = 2;

            foreach (var x in lista)
            {
                ws.Cell(row, 1).Value = x.DocNumTicket;
                ws.Cell(row, 2).Value = x.GuiaRemision?.Replace(Environment.NewLine, " , ").Trim();
                ws.Cell(row, 3).Value = x.GuiaTransportista;
                ws.Cell(row, 4).Value = x.CardCode;
                ws.Cell(row, 5).Value = x.CardName;
                ws.Cell(row, 6).Value = x.Direccion1;
                ws.Cell(row, 7).Value = x.Direccion2;
                ws.Cell(row, 8).Value = x.Agencia;
                ws.Cell(row, 9).Value = x.DistritoTransporte;
                ws.Cell(row, 10).Value = x.ModoEnvio;
                ws.Cell(row, 11).Value = x.Cajas;
                ws.Cell(row, 12).Value = x.Peso;
                ws.Cell(row, 13).Value = x.MontoFlete;
                ws.Cell(row, 14).Value = x.Contacto;
                ws.Cell(row, 15).Value = x.Telefono;
                ws.Cell(row, 16).Value = x.Estado;
                ws.Cell(row, 17).Value = x.Observacion;
            
                if (model.esHojaRuta == false)
                {
                    ws.Cell(row, 18).Value = x.Placa;
                    ws.Cell(row, 19).Value = x.Factura;
                    ws.Cell(row, 20).Value = x.FechaRecojo?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 21).Value = x.HoraPac;
                    ws.Cell(row, 22).Value = x.FechaEntrega?.ToString("dd/MM/yyyy");
                }

                row++;
            }

            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            ws.SheetView.FreezeRows(1);
            ws.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.None;
            ws.Cells().Style.Border.InsideBorder = XLBorderStyleValues.None;
            ws.ShowGridLines = false;

            workbook.SaveAs(stream);

            // Nombre del archivo según esHojaRuta
            string fileName = model.esHojaRuta == true
                ? $"TicketsHojaRuta_{model.DocNumHojaRuta}_{DateTime.Now:yyyyMMdd}.xlsx"
                : $"TicketsRecogidos_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        [HttpGet]
        public IActionResult ObtenerImagenes(int docNum)
        {
            try
            {
                var lista = ticketsService.ObtenerImagenesLidermax(docNum);

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
        public async Task<IActionResult> ExcluirTicket([FromBody] TicketSeleccionadoDto model)
        {
            try
            {
                var result = ticketsService.ExcluirTicket(model); 

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


       

    }
}
