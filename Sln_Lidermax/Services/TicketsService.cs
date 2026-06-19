using Azure.Core;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Models;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Repositories;
using X.PagedList;

namespace Sln_Lidermax.Services
{
    public class TicketsService : ITicketsService
    {
        private readonly ITicketsRepository ticketsRepository;
        private readonly DapperContext dapperContext;
        private readonly ITicketsOperarioRepository ticketsOperarioRepository;

        public TicketsService(ITicketsRepository ticketsRepository, DapperContext dapperContext, ITicketsOperarioRepository ticketsOperarioRepository ) {
            this.ticketsRepository = ticketsRepository;
            this.dapperContext = dapperContext;
            this.ticketsOperarioRepository = ticketsOperarioRepository;
        }

        public async Task<IPagedList<TicketsModel>> ListadoTickets(FiltrosTicketsModel model)
        {
            return await ticketsRepository.ListadoTickets(model);
        }

        public async Task<bool> InsertarTicketsRecogidos(RecogerTicketsModel request)
        {
            using var con = new SqlConnection(dapperContext.connectionString);
            await con.OpenAsync();

            using var tx = con.BeginTransaction();

            try
            {
                foreach (var ticket in request.Tickets)
                {

                    var estadoActual = await ticketsRepository.ObtenerEstadoTicket(ticket.DocEntryHojaRuta,ticket.Linea,ticket.DocEntryTicket, con, tx);

                    if (estadoActual == "LIBERADO")
                    {
                        throw new Exception($"El ticket {ticket.DocNumTicket} está LIBERADO y no se puede recoger.");
                    }

                    var resultInsert = await ticketsRepository.InsertarTicketsRecogidos(ticket.DocEntryHojaRuta,ticket.Linea, ticket.DocEntryTicket, con, tx);

                    if (!resultInsert)
                    {
                        throw new Exception("Error insertando ticket");
                    }

                    bool actualizarExcluido = true;
                    if(ticket.EntregaPedido == "RECOJO")
                    {
                        TicketSeleccionadoDto obj = new TicketSeleccionadoDto()
                        {
                            DocEntryHojaRuta = ticket.DocEntryHojaRuta,
                            Linea = ticket.Linea,
                            DocEntryTicket = ticket.DocEntryTicket,
                            Excluido = true
                        };
                        actualizarExcluido = await ticketsRepository.ExcluirTicket(obj, con, tx); //falta transaccion
                    }
                    if (!resultInsert)
                    {
                        throw new Exception("Error actualizando excluido");
                    }

                    var conteo = await ticketsRepository.ObtenerConteoTickets(ticket.DocEntryHojaRuta, new[] { "RECOGIDO", "ENVIADO","LIBERADO" }, con, tx);

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
                throw;
                //return false;
            }
        }

        public async Task<IPagedList<TicketsModel>> ListadoTicketsRecogidos(FiltrosTicketsModel model)
        {
            return await ticketsRepository.ListadoTicketsRecogidos(model);
        }

        public async Task<bool> ActualizarFechaDespacho(TicketsModel model)
        {
            return await ticketsRepository.ActualizarFechaDespacho(model);
        }
        public async Task<bool> ActualizarGuiaTransportista(TicketsModel model)
        {
            return await ticketsRepository.ActualizarGuiaTransportista(model);
        }
        public async Task<bool> ActualizarObservacion(TicketSeleccionadoDto model)
        {
            return await ticketsRepository.ActualizarObservacion(model);
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

        public async Task<bool> ActualizarDatos(SubirImagenesModel request)
        {
            string rutaBase = @"C:\COBEFARWEBFILES\DespachoLidermax";

            if (!Directory.Exists(rutaBase))
            {
                Directory.CreateDirectory(rutaBase);
            }

            var archivosGuardados = new List<string>();

            using SqlConnection con = new SqlConnection(dapperContext.connectionString);

            await con.OpenAsync();

            using var tx = con.BeginTransaction();

            try
            {
                // =========================================
                // OBTENER ESTADO ACTUAL DEL TICKET
                // =========================================

                var ticketActual = await ticketsRepository.ObtenerTicket(request.DocEntryHojaRuta, request.Linea, request.DocEntryTicket, con, tx);

                bool yaEraPagado = ticketActual?.EstadoPago?.Equals("PAGADO", StringComparison.OrdinalIgnoreCase) == true;

                bool ahoraEsPagado = request.EstadoPago?.Equals("PAGADO", StringComparison.OrdinalIgnoreCase) == true;

                // =========================================
                // IMG1 OPCIONAL
                // =========================================

                if (request.Img1 != null && request.Img1.Length > 0)
                {

                    string nombreBase = $"{request.DocNumTicket}_Comprobante";

                    EliminarArchivosExistentes(rutaBase, nombreBase);

                    string path1 = Path.Combine(rutaBase,$"{nombreBase}{Path.GetExtension(request.Img1.FileName)}");

                    await GuardarArchivo(request.Img1, path1);

                    archivosGuardados.Add(path1);
                }

                // =========================================
                // IMG2 OPCIONAL
                // =========================================

                if (request.Img2 != null && request.Img2.Length > 0)
                {

                    string nombreBase = $"{request.DocNumTicket}_Pedido";

                    EliminarArchivosExistentes(rutaBase, nombreBase);

                    string path2 = Path.Combine(rutaBase,$"{nombreBase}{Path.GetExtension(request.Img2.FileName)}");

                    await GuardarArchivo(request.Img2, path2);

                    archivosGuardados.Add(path2);
                }

                // =========================================
                // VALIDACIÓN ESTADO PAGO
                // =========================================

                if (ahoraEsPagado)
                {
                    // =====================================
                    // SI ES LA PRIMERA VEZ QUE PASA A PAGADO
                    // =====================================

                    if (!yaEraPagado)
                    {
                        // IMAGEN OBLIGATORIA
                        //if (request.ImgPago == null || request.ImgPago.Length == 0)
                        //{
                        //    throw new Exception("La imagen de pago es obligatoria");
                        //}

                        //FACTURA OBLIGATORIA
                        if (string.IsNullOrEmpty(request.Factura))
                        {
                            throw new Exception("El campo de factura es obligatorio");
                        }

                        // MONTO OBLIGATORIO
                        if (request.MontoFlete == null || request.MontoFlete <= 0)
                        {
                            throw new Exception("El monto flete es obligatorio");
                        }
                    }
                    else
                    {
                        if (request.MontoFlete <= 0)
                        {
                            throw new Exception("El monto flete debe ser mayor a 0");
                        }
                        if (string.IsNullOrEmpty(request.Factura))
                        {
                            throw new Exception("El campo de factura es obligatorio");
                        }
                    }

                    // =====================================
                    // SI SUBIÓ NUEVA IMAGEN -> ACTUALIZAR
                    // =====================================

                    //if (request.ImgPago != null && request.ImgPago.Length > 0)
                    //{
                    //    string pathPago = Path.Combine(rutaBase,$"{request.DocNumTicket}_Pago{Path.GetExtension(request.ImgPago.FileName)}");

                    //    await GuardarArchivo(request.ImgPago, pathPago);

                    //    archivosGuardados.Add(pathPago);
                    //}
                }
                else
                {
                    if(request.EstadoPago != null)
                    {
                        // =====================================
                        // SI CAMBIA A POR PAGAR
                        // =====================================

                        request.MontoFlete = null;
                        request.Factura = null;

                        // ELIMINAR IMAGEN ANTERIOR
                        //string archivoPagoAnterior =Directory.GetFiles(rutaBase,$"{request.DocNumTicket}_Pago.*").FirstOrDefault();

                        //if (!string.IsNullOrWhiteSpace(archivoPagoAnterior))
                        //{
                        //    File.Delete(archivoPagoAnterior);
                        //}

                        await ticketsOperarioRepository.ActualizarMontoFlete(request, con, tx);
                        await ticketsOperarioRepository.ActualizarFactura(request, con, tx);
                    }       
                }

                // =========================================
                // ACTUALIZAR ESTADO PAGO
                // =========================================

                var resultEstadoPago = await ticketsOperarioRepository.ActualizarEstadoPago(request, con, tx);

                // =========================================
                // ACTUALIZAR MONTO SOLO SI EXISTE
                // =========================================

                bool resultMonto = true;
                if (request.MontoFlete.HasValue && request.MontoFlete > 0)
                {
                    resultMonto = await ticketsOperarioRepository.ActualizarMontoFlete(request, con, tx);
                }

                bool resultFactura = true;
                if(request.Factura != null)
                {
                    resultFactura = await ticketsOperarioRepository.ActualizarFactura(request, con, tx);
                }

                bool resultTicketEnviado = true;
                if (ticketActual.Estado =="RECOGIDO")
                {
                    resultTicketEnviado = await ticketsRepository.ActualizarEstadoEnviado(request.DocEntryHojaRuta, request.Linea, request.DocEntryTicket, con, tx);
                }

                // =========================================
                // VALIDAR RESULTADO
                // =========================================

                bool resultado = resultEstadoPago && resultMonto && resultFactura && resultTicketEnviado;

                if (!resultado)
                {
                    throw new Exception("Error actualizando datos");
                }

                tx.Commit();

                return true;
            }
            catch
            {
                tx.Rollback();

                // BORRAR ARCHIVOS SI FALLA
                foreach (var archivo in archivosGuardados)
                {
                    if (File.Exists(archivo))
                    {
                        File.Delete(archivo);
                    }
                }

                throw;
            }
        }

        private async Task GuardarArchivo(IFormFile file, string path)
        {
            using var stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);

            if (!File.Exists(path))
                throw new Exception($"Error guardando archivo: {path}");
        }

        private void EliminarArchivosExistentes(string rutaBase, string nombreBase)
        {
            var archivosExistentes = Directory.GetFiles(rutaBase, $"{nombreBase}.*");

            foreach (var archivo in archivosExistentes)
            {
                if (File.Exists(archivo))
                {
                    File.Delete(archivo);
                }
            }
        }

        public async Task<bool> SubirImagenes(SubirImagenesModel request)
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
                   
                var resultTicketEnviado = await ticketsRepository.ActualizarEstadoEnviado(request.DocEntryHojaRuta,request.Linea, request.DocEntryTicket, con, tx);
             
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

        public async Task<List<TicketsModel>> ListadoTicketsExcel(FiltrosTicketsModel model)
        {
            return await ticketsRepository.ListadoTicketsExcel(model);
        }

        public async Task<List<TicketsModel>> ListadoTicketsRecogidosExcel(FiltrosTicketsModel model)
        {
            return await ticketsRepository.ListadoTicketsRecogidosExcel(model);
        }

        public List<object> ObtenerImagenesLidermax(int docNumTicket, string tipoFiltro = null)
        {
            List<dynamic> arrImg = new List<dynamic>();

            string ruta = @"C:\COBEFARWEBFILES\DespachoLidermax";

            if (Directory.Exists(ruta))
            {
                var archivos = Directory.GetFiles(ruta, docNumTicket + "_*");

                foreach (var archivo in archivos)
                {
                    string extension = Path.GetExtension(archivo).ToLower();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {
                        string nombreArchivo = Path.GetFileNameWithoutExtension(archivo);
                        string tipo = "Desconocido";
                        int orden = 99;

                        if (nombreArchivo.ToLower().Contains("comprobante"))
                        {
                            tipo = "Comprobante";
                            orden = 1;
                        }else if (nombreArchivo.ToLower().Contains("pedido"))
                        {
                            tipo = "Pedido";
                            orden = 2;
                        }
                        else if (nombreArchivo.ToLower().Contains("pago"))
                        {
                            tipo = "Pago";
                            orden = 3;
                        }

                        if (!string.IsNullOrEmpty(tipoFiltro))
                        {
                            if (!tipo.Equals(tipoFiltro, StringComparison.OrdinalIgnoreCase))
                                continue;
                        }


                        byte[] img = File.ReadAllBytes(archivo);
                        string base64 = Convert.ToBase64String(img);
                        string ext = extension.Replace(".", "");

                        arrImg.Add(new
                        {
                            imagen = $"data:image/{ext};base64,{base64}",
                            tipo = tipo,
                            orden = orden
                        });
                    }
                }
            }
            //return arrImg;

            return arrImg
               .OrderBy(x => x.orden)
               .Select(x => new
               {
                   imagen = x.imagen,
                   tipo = x.tipo
               })
               .Cast<object>()
               .ToList();
        }

        //Excluir
        public async Task<bool> ExcluirTicket(TicketSeleccionadoDto model)
        {
            using SqlConnection con = new SqlConnection(dapperContext.connectionString);
            await con.OpenAsync();
            using var tx = con.BeginTransaction();
            try
            {
                var result= await ticketsRepository.ExcluirTicket(model,con,tx);

                tx.Commit();

                return result;
            }
            catch
            {
                tx.Rollback();
                return false;
            }
        }

    }
}
