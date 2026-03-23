using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sln_Lidermax.Dtos;
using Sln_Lidermax.Interfaces;
using System.Drawing;

namespace Sln_Lidermax.Controllers
{
    [Authorize]
    public class HojasRutaController : Controller
    {
        private readonly IHojasRutaService hojasRutaService;

        public HojasRutaController(IHojasRutaService hojasRutaService )
        {
            this.hojasRutaService = hojasRutaService;
        }
        public async Task<IActionResult> ListadoHojasRuta(FiltrosHojasRutaDto model)
        {

            var listaHojasRuta = await hojasRutaService.ListadoHojasRutaPaginados(model);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") //nos dice si es una peticion AJAX
            {
                return PartialView("_TablaHojasRuta", listaHojasRuta);
            }

            return View(listaHojasRuta);
        }

        public async Task<IActionResult> ExportarExcel(FiltrosHojasRutaDto model)
        {
            var lista = await hojasRutaService.ListadoHojasRutaExcel(model);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Hojas de ruta");

      
            // CABECERAS
            ws.Cell(1, 1).Value = "Nro Hoja Ruta";
            ws.Cell(1, 2).Value = "Tipo Ruta";
            ws.Cell(1, 3).Value = "Tiempo Pactado";
            ws.Cell(1, 4).Value = "Nro Cajas";
            ws.Cell(1, 5).Value = "Estado";
        
            ws.Range("A1:E1").Style.Font.Bold = true;
    
            int row = 2;

            foreach (var x in lista)
            {
                ws.Cell(row, 1).Value = x.DocNum;
                ws.Cell(row, 2).Value = x.TipoRuta;
                ws.Cell(row, 3).Value = x.TiempoPac;
                ws.Cell(row, 4).Value = x.Cajas;
                ws.Cell(row, 5).Value = x.Estado;
                
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

    }
}
