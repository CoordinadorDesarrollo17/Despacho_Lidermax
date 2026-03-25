using Azure.Core;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Dtos;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Repositories;
using X.PagedList;

namespace Sln_Lidermax.Services
{
    public class TicketsService : ITicketsService
    {
        private readonly ITicketsRepository ticketsRepository;
        private readonly DapperContext dapperContext;

        public TicketsService(ITicketsRepository ticketsRepository, DapperContext dapperContext) {
            this.ticketsRepository = ticketsRepository;
            this.dapperContext = dapperContext;
        }

        public async Task<IPagedList<TicketsDto>> ListadoTickets(FiltrosTicketsDto model)
        {
            return await ticketsRepository.ListadoTickets(model);
        }

        public async Task<bool> InsertarTicketsRecogidos(RecogerTicketsDto request)
        {
            using var con = new SqlConnection(dapperContext.connectionString);
            await con.OpenAsync();

            using var tx = con.BeginTransaction();

            try
            {
                foreach (var ticket in request.Tickets)
                {
                 
                    var resultInsert = await ticketsRepository.InsertarTicketsRecogidos(ticket.DocEntryTicket, con, tx);

                    if (!resultInsert)
                    {
                        throw new Exception("Error insertando ticket");
                    }
                   
                    var conteo = await ticketsRepository.ObtenerConteoTickets(ticket.DocEntryHojaRuta, new[] { "RECOGIDO", "ENVIADO" }, con, tx);

                    if (conteo.TotalTickets == conteo.TicketsObtenidos)
                    {
                        await ticketsRepository.ActualizarEstadoHojaRuta(ticket.DocEntryHojaRuta, "RECOGIDO", con, tx);
                    }
                    else
                    {
                        await ticketsRepository.ActualizarEstadoHojaRuta(ticket.DocEntryHojaRuta, "RECOGIDO PARCIAL", con, tx);
                    }
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                return false;
            }
        }

        public async Task<IPagedList<TicketsDto>> ListadoTicketsRecogidos(FiltrosTicketsDto model)
        {
            return await ticketsRepository.ListadoTicketsRecogidos(model);
        }

        public async Task<bool> ActualizarFechaDespacho(TicketsDto model)
        {
            return await ticketsRepository.ActualizarFechaDespacho(model);
        }
        public async Task<bool> ActualizarGuiaTransportista(TicketsDto model)
        {
            return await ticketsRepository.ActualizarGuiaTransportista(model);
        }

        public async Task<bool> DevolverTicket(TicketSeleccionadoDto model)
        {
            using SqlConnection con = new SqlConnection(dapperContext.connectionString);
            await con.OpenAsync();
            using var tx = con.BeginTransaction();
            try
            {
                var resultTicketDevuelto = await ticketsRepository.DevolverTicket(model, con, tx);

                tx.Commit();

                return resultTicketDevuelto;
            }
            catch
            {
                tx.Rollback();
                return false;
            }
        }
        public async Task<bool> EntregarTicket(TicketSeleccionadoDto request)
        {
            using SqlConnection con = new SqlConnection(dapperContext.connectionString);
            await con.OpenAsync();
            using var tx = con.BeginTransaction();
            try
            {
                var resultTicketEntregado = await ticketsRepository.ActualizarEstadoEntregado(request, con, tx);

                var conteo = await ticketsRepository.ObtenerConteoTickets(request.DocEntryHojaRuta, new[] { "ENTREGADO", "DEVOLUCION" }, con, tx);

                bool resultFinal;

                if (conteo.TotalTickets == conteo.TicketsObtenidos)
                {
                    resultFinal = await ticketsRepository.ActualizarEstadoHojaRuta(request.DocEntryHojaRuta, "TERMINADO", con, tx);
                }
                else
                {
                    resultFinal = resultTicketEntregado;
                }

                tx.Commit();

                return resultTicketEntregado;
            }
            catch
            {
                tx.Rollback();
                return false;
            }
        }

        public async Task<bool> SubirImagenes(SubirImagenesDto request)
        {
            string rutaBase = @"C:\COBEFARWEBFILES\DespachoLidermax";

            if (!Directory.Exists(rutaBase))
            {
                Directory.CreateDirectory(rutaBase);
            }
               
            string nombreImg1 = $"{request.DocNumTicket}_Comprobante{Path.GetExtension(request.Img1.FileName)}";
            string nombreImg2 = $"{request.DocNumTicket}_Pedido{Path.GetExtension(request.Img2.FileName)}";

            string path1 = Path.Combine(rutaBase, nombreImg1);
            string path2 = Path.Combine(rutaBase, nombreImg2);

            using SqlConnection con = new SqlConnection(dapperContext.connectionString);
            await con.OpenAsync();

            using var tx = con.BeginTransaction();

            try
            {
                using (var stream = new FileStream(path1, FileMode.Create))
                {
                    await request.Img1.CopyToAsync(stream);
                }
                   
                using (var stream = new FileStream(path2, FileMode.Create))
                {
                    await request.Img2.CopyToAsync(stream);
                }
                   
                if (!File.Exists(path1) || !File.Exists(path2))
                {
                    throw new Exception("Error guardando imágenes");
                }
                   
                var resultTicketEnviado = await ticketsRepository.ActualizarEstadoEnviado(request.DocEntryTicket, con, tx);

               

                tx.Commit();

                return resultTicketEnviado;
            }
            catch
            {
                tx.Rollback();

                if (File.Exists(path1)) File.Delete(path1);
                if (File.Exists(path2)) File.Delete(path2);

                return false;
            }
        }

        public async Task<List<TicketsDto>> ListadoTicketsExcel(FiltrosTicketsDto model)
        {
            return await ticketsRepository.ListadoTicketsExcel(model);
        }

        public async Task<List<TicketsDto>> ListadoTicketsRecogidosExcel(FiltrosTicketsDto model)
        {
            return await ticketsRepository.ListadoTicketsRecogidosExcel(model);
        }

        public List<object> ObtenerImagenesLidermax(int docNumTicket)
        {
            List<object> arrImg = new List<object>();

            string ruta = @"C:\COBEFARWEBFILES\DespachoLidermax";

            if (Directory.Exists(ruta))
            {
                var archivos = Directory.GetFiles(ruta, docNumTicket + "_*");

                foreach (var archivo in archivos)
                {
                    string extension = Path.GetExtension(archivo).ToLower();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {
                        byte[] img = File.ReadAllBytes(archivo);
                        string base64 = Convert.ToBase64String(img);
                        string ext = extension.Replace(".", "");

                        string nombreArchivo = Path.GetFileNameWithoutExtension(archivo);
                        string tipo = "Desconocido";

                        if (nombreArchivo.ToLower().Contains("comprobante"))
                            tipo = "Comprobante";
                        else if (nombreArchivo.ToLower().Contains("pedido"))
                            tipo = "Pedido";

                        arrImg.Add(new
                        {
                            imagen = $"data:image/{ext};base64,{base64}",
                            tipo = tipo
                        });
                    }
                }
            }
            return arrImg;
        }
    }
}
