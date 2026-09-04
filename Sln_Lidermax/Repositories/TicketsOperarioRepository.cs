using Dapper;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace Sln_Lidermax.Repositories
{
    public class TicketsOperarioRepository : ITicketsOperarioRepository
    {
        private readonly DapperContext dapperContext;

        public TicketsOperarioRepository(DapperContext dapperContext)
        {
            this.dapperContext = dapperContext;
        }

        private async Task<IEnumerable<TicketsModel>> ObtenerTicketsOperario(FiltrosTicketsModel model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"   SELECT TOP 200 
                            tr.DocEntry AS DocEntryHojaRuta, tr.Linea, tk.DocEntry AS DocEntryTicket, tk.DocNum AS DocNumTicket,
                            tk.CardCode,tk.CardName,tr.Guias AS GuiaRemision,tr.Cajas, CASE WHEN rfd.Agencia IS NOT NULL THEN rfd.Agencia ELSE tk.Agencia END AS Agencia, 
                            rfd.Estado , rfd.EstadoPago
                            FROM al.RRU0 AS tr 
                            LEFT JOIN al.ORRU AS r ON r.DocEntry = tr.DocEntry 
                            LEFT JOIN vt.ORTV AS tk ON tr.DocEntryTicket = tk.DocEntry 
                            LEFT JOIN tmp.registro_fecha_despacho AS rfd ON rfd.DocEntryHojaRuta=tr.DocEntry AND rfd.Linea= tr.Linea AND rfd.DocEntryTicket = tr.DocEntryTicket
                            WHERE ( (r.TipoRuta = 'VG' AND r.TransDesc LIKE '%LIDERMAX%') OR (r.TipoRuta='VD' AND tk.LugarDestino = 'DOMICILIO' AND tk.EntregaPedido IN ('PROVINCIA','RECOJO') ) )                  
                              AND (rfd.Estado IS NOT NULL AND rfd.Estado <> '' )
                              AND tr.Estado <> 'LIBERADO' 
                              AND rfd.Excluido = 0
                              AND (r.Trans2Desc LIKE @NombreCompleto OR rfd.Conductor LIKE @NombreCompleto )
                              AND CONCAT(tk.DocNum,tk.CardCode,tk.CardName,tr.Guias,tr.Cajas,tk.Agencia) LIKE @Buscar
                            GROUP BY 
                            tr.DocEntry , tr.Linea, tk.DocEntry , tk.DocNum ,
                            tk.CardCode , tk.CardName,rfd.FechaRecojo,tr.Guias,tk.DocNum,tr.Cajas,tk.Agencia , rfd.Estado , rfd.EstadoPago , rfd.Agencia
                            ORDER BY FechaRecojo DESC ";

            var result = await xCon.QueryAsync<TicketsModel>(sql, new { NombreCompleto = "%" + model.NombreCompleto + "%", Buscar = "%" + model.Buscar + "%" });
            return result;
        }

        public async Task<IPagedList<TicketsModel>> ListadoTicketsOperario(FiltrosTicketsModel model)
        {
            var result = await ObtenerTicketsOperario(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<bool> ActualizarTransportista(int DocEntryHojaRuta, int Linea, int DocEntryTicket, string Transportista)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                    SET Agencia = @Transportista
                    WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket ";

            var result = await xCon.ExecuteAsync(sql, new
            {
                DocEntryHojaRuta ,
                Linea ,
                DocEntryTicket ,
                Transportista
            });

            return result > 0;
        }

        public async Task<bool> ActualizarEstadoPago(SubirImagenesModel request, SqlConnection con, SqlTransaction tx)
        {
            var sql = @" UPDATE [tmp].[registro_fecha_despacho]
                SET EstadoPago = @EstadoPago
                WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new { request.DocEntryHojaRuta, request.Linea , request.DocEntryTicket,request.EstadoPago }, tx);

            return result > 0 ;
        }

        public async Task<bool> ActualizarMontoFlete(SubirImagenesModel request, SqlConnection con, SqlTransaction tx)
        {
            var sql = @" UPDATE [tmp].[registro_fecha_despacho]
                SET  MontoFlete = @MontoFlete
                WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new { request.DocEntryHojaRuta, request.Linea, request.DocEntryTicket, request.MontoFlete }, tx);

            return result > 0;
        }


        public async Task<bool> ActualizarFactura(SubirImagenesModel request, SqlConnection con, SqlTransaction tx)
        {
            var sql = @" UPDATE [tmp].[registro_fecha_despacho]
                SET  Factura = @Factura
                WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new { request.DocEntryHojaRuta, request.Linea, request.DocEntryTicket, request.Factura }, tx);

            return result > 0;
        }       
    }
}
