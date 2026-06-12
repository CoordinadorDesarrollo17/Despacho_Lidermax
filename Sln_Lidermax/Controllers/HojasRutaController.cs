using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sln_Lidermax.Models;
using Sln_Lidermax.Interfaces;
using System.Drawing;
using System.IO;

namespace Sln_Lidermax.Controllers
{
    [Authorize(Roles = "MANAGER,SLID")]
    public class HojasRutaController : Controller
    {
        private readonly IHojasRutaService hojasRutaService;

        public HojasRutaController(IHojasRutaService hojasRutaService )
        {
            this.hojasRutaService = hojasRutaService;
        }
        public async Task<IActionResult> ListadoHojasRuta(FiltrosHojasRutaModel model)
        {

            var listaHojasRuta = await hojasRutaService.ListadoHojasRutaPaginados(model);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") //nos dice si es una peticion AJAX
            {
                return PartialView("_TablaHojasRuta", listaHojasRuta);
            }

            return View(listaHojasRuta);
        }
      
        public async Task<IActionResult> ExportarExcel(FiltrosHojasRutaModel model)
        {
            var lista = await hojasRutaService.ListadoHojasRutaExcel(model);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Hojas de ruta");

      
            // CABECERAS
            ws.Cell(1, 1).Value = "Nro Hoja Ruta";
            ws.Cell(1, 2).Value = "Tipo Ruta";
            ws.Cell(1, 3).Value = "Placa";
            ws.Cell(1, 4).Value = "Fecha Pactada";
            ws.Cell(1, 5).Value = "Hora Pactada";
            ws.Cell(1, 6).Value = "Nro Cajas";
            ws.Cell(1, 7).Value = "Estado";
        
            ws.Range("A1:G1").Style.Font.Bold = true;
    
            int row = 2;

            foreach (var x in lista)
            {
                ws.Cell(row, 1).Value = x.DocNum;
                ws.Cell(row, 2).Value = x.TipoRuta;
                ws.Cell(row, 3).Value = x.Placa;
                ws.Cell(row, 4).Value = x.TiempoPac;
                ws.Cell(row, 5).Value = x.HoraPac;
                ws.Cell(row, 6).Value = x.Cajas;
                ws.Cell(row, 7).Value = x.Estado;
                
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            ws.SheetView.FreezeRows(1);         // Congelar primera fila
            ws.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.None;
            ws.Cells().Style.Border.InsideBorder = XLBorderStyleValues.None;
            ws.ShowGridLines = false;           // Ocultar líneas de cuadrícula

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HojasRuta_{DateTime.Now:yyyyMMddHHmm}.xlsx"
            );
        }
        public async Task<IActionResult> ExportarPdfPorHojaRuta(int docEntryHojaRuta, int docNumHojaRuta,string tiempoPac, string horaPac)
        {
            var data = await hojasRutaService.ListadoTicketsPorHojasRutaPdf(docEntryHojaRuta);

            ViewBag.DocNum = docNumHojaRuta;
            ViewBag.TiempoPac = tiempoPac;
            ViewBag.HoraPac = horaPac;
            ViewBag.Placa = data.FirstOrDefault()?.Placa;

            return View("PdfHojaRuta", data);
        }
        public async Task<IActionResult> ExportarExcelPorHojaRuta(int docEntryHojaRuta, int docNumHojaRuta)
        {
            var lista = await hojasRutaService.ListadoTicketsPorHojasRutaExcel(docEntryHojaRuta);

            foreach (var item in lista)
            {
                if (!string.IsNullOrWhiteSpace(item.Guias))
                {
                    item.Guias = item.Guias.Replace("\r\n", ",");
                }
            }

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Hoja de ruta");

            // CABECERAS
            ws.Cell(1, 1).Value = "Tiempo Pactado";
            ws.Cell(1, 2).Value = "Socio";
            ws.Cell(1, 3).Value = "EnvioAgencia";
            ws.Cell(1, 4).Value = "Guias";
            ws.Cell(1, 5).Value = "OrdenCompra";
            ws.Cell(1, 6).Value = "Ruc";
            ws.Cell(1, 7).Value = "Direccion1";
            ws.Cell(1, 8).Value = "Direccion2";
            ws.Cell(1, 9).Value = "Departamento1";
            ws.Cell(1, 10).Value = "Departamento2";
            ws.Cell(1, 11).Value = "PersonaRecojo";
            ws.Cell(1, 12).Value = "Documento";
            ws.Cell(1, 13).Value = "Telefono";
            ws.Cell(1, 14).Value = "Cajas";
            ws.Cell(1, 15).Value = "Peso";
            ws.Cell(1, 16).Value = "Transportista";
            ws.Cell(1, 17).Value = "ModoEnvio";
            ws.Cell(1, 18).Value = "Factura";
            ws.Cell(1, 19).Value = "Corte";
            ws.Cell(1, 20).Value = "Ruta1";
            ws.Cell(1, 21).Value = "Ruta2";

            ws.Range("A1:U1").Style.Font.Bold = true;

            int row = 2;

            foreach (var x in lista)
            {
                ws.Cell(row, 1).Value = x.TiempoPac;
                ws.Cell(row, 2).Value = x.Socio;
                ws.Cell(row, 3).Value = x.EnvioAgencia;
                ws.Cell(row, 4).Value = x.Guias;
                ws.Cell(row, 5).Value = x.DocNumTicket;
                ws.Cell(row, 6).Value = x.CardCode;
                ws.Cell(row, 7).Value = x.Calle1;
                ws.Cell(row, 8).Value = x.Calle2;
                ws.Cell(row, 9).Value = x.Departamento1;
                ws.Cell(row, 10).Value = x.Departamento2;
                ws.Cell(row, 11).Value = x.NombrePer;
                ws.Cell(row, 12).Value = x.DocPer;
                ws.Cell(row, 13).Value = x.TelfPer;
                ws.Cell(row, 14).Value = x.Cajas;
                ws.Cell(row, 15).Value = x.Peso;
                ws.Cell(row, 16).Value = x.Transportista;
                ws.Cell(row, 17).Value = x.ModoEnvio;
                ws.Cell(row, 18).Value = x.Factura;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            ws.SheetView.FreezeRows(1);         // Congelar primera fila
            ws.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.None;
            ws.Cells().Style.Border.InsideBorder = XLBorderStyleValues.None;
            ws.ShowGridLines = false;           // Ocultar líneas de cuadrícula

            workbook.SaveAs(stream);


            return File(
              stream.ToArray(),
              "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
              $"HojaRuta_{docNumHojaRuta}.xlsx"
          );
        }


    }
}
