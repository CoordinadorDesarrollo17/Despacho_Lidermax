using Microsoft.Data.SqlClient;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using Sln_Lidermax.Repositories;
using X.PagedList;

namespace Sln_Lidermax.Services
{
    public class TicketsOperarioService : ITicketsOperarioService
    {
        private readonly ITicketsOperarioRepository ticketsOperarioRepository;
        private readonly DapperContext dapperContext;

        public TicketsOperarioService(ITicketsOperarioRepository ticketsOperarioRepository , DapperContext dapperContext)
        {
            this.ticketsOperarioRepository = ticketsOperarioRepository;
            this.dapperContext = dapperContext;
        }

        public async Task<IPagedList<TicketsModel>> ListadoTicketsOperario(FiltrosTicketsModel model)
        {
            return await ticketsOperarioRepository.ListadoTicketsOperario(model);
        }

        public async Task<bool> ActualizarTransportista(TicketsModel model)
        {
            return await ticketsOperarioRepository.ActualizarTransportista(model);
        }


        public async Task<bool> RegistrarEstadoPago(SubirImagenesModel request)
        {
            string rutaBase = @"D:\COBEFARWEBFILES\DespachoLidermax";

            if (!Directory.Exists(rutaBase))
            {
                Directory.CreateDirectory(rutaBase);
            }

            string pathPago = string.Empty;

            using SqlConnection con = new SqlConnection(dapperContext.connectionString);
            await con.OpenAsync();

            using var tx = con.BeginTransaction();

            try
            {
                // SOLO guardar imagen si el estado es PAGADO
                if (request.EstadoPago == "PAGADO")
                {
                    // Validar imagen
                    if (request.ImgPago == null)
                    {
                        throw new Exception("Debe enviar imagen de pago");
                    }

                    string nombreImgPago = $"{request.DocNumTicket}_Pago{Path.GetExtension(request.ImgPago.FileName)}";

                    pathPago = Path.Combine(rutaBase, nombreImgPago);

                    using (var stream = new FileStream(pathPago, FileMode.Create))
                    {
                        await request.ImgPago.CopyToAsync(stream);
                    }

                    if (!File.Exists(pathPago))
                    {
                        throw new Exception("Error guardando imagen pago");
                    }
                }

                var resultTicketEnviado = await ticketsOperarioRepository.ActualizarEstadoPago(request, con, tx);

                tx.Commit();

                return resultTicketEnviado;
            }
            catch
            {
                tx.Rollback();
                if (File.Exists(pathPago)) File.Delete(pathPago);
                return false;
            }
        }

    }
}
