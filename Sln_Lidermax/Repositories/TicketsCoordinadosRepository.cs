using Dapper;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace Sln_Lidermax.Repositories
{
    public class TicketsCoordinadosRepository : ITicketsCoordinadosRepository
    {
        private readonly DapperContext dapperContext;

        public TicketsCoordinadosRepository(DapperContext dapperContext)
        {
            this.dapperContext = dapperContext;
        }

        private async Task<IEnumerable<TicketsCoordinados>> ObtenerTicketsCoordinados(FiltrosTicketsModel model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"     
                        SELECT TOP 200            
                            tk.DocNum AS DocNumTicket,
                            r.DocNum AS DocNumHojaRuta,
                            r.FechaDoc AS FechaDocumento,
                            tk.CardName AS Socio,
                            rfd.Placa AS Placa,
                            rfd.Conductor AS Conductor,
                            tk.EntregaPedido AS EntregaPedido,
                            tk.DetallePedido AS DetallePedido,
                            tk.RangoHorario AS RangoHorario,
                            rfd.DocEntryHojaRuta AS DocEntryHojaRuta,
                            rfd.Linea AS Linea,
                            rfd.DocEntryTicket AS DocEntryTicket,
                            rfd.Estado AS Estado
                        FROM al.ORRU AS r
                        INNER JOIN al.RRU0 AS tr ON r.DocEntry = tr.DocEntry AND tr.Estado <> 'LIBERADO'
                        LEFT JOIN vt.ORTV AS tk ON tk.DocEntry = tr.DocEntryTicket 
                        LEFT JOIN tmp.registro_fecha_despacho AS rfd ON rfd.DocEntryHojaRuta=tr.DocEntry AND rfd.Linea= tr.Linea AND rfd.DocEntryTicket = tr.DocEntryTicket
                        WHERE (r.TipoRuta = 'VD'  AND tk.LugarDestino = 'DOMICILIO' AND tk.EntregaPedido IN ('PROVINCIA','RECOJO'))
                             AND (rfd.Estado IS NOT NULL AND rfd.Estado <> '' )
                         AND CONCAT(r.DocNum,tk.DocNum,tk.CardName,rfd.Placa,rfd.Conductor) LIKE @Buscar
                        ORDER BY r.DocEntry DESC ";
            var result = await xCon.QueryAsync<TicketsCoordinados>(sql,new { Buscar = "%" + model.Buscar + "%" });
            return result;
        }

        public async Task<IPagedList<TicketsCoordinados>> ListadoTicketsCoordinadosPaginados(FiltrosTicketsModel model)
        {
            var result = await ObtenerTicketsCoordinados(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<IEnumerable<string>> ObtenerConductores()
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);
            return await xCon.QueryAsync<string>("SELECT Conductor FROM al.ConductorYPlaca ORDER BY Conductor");
        }

        public async Task<IEnumerable<string>> ObtenerPlacas()
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);
            return await xCon.QueryAsync<string>("SELECT Placa FROM al.ConductorYPlaca ORDER BY Placa");
        }

        public async Task<bool> ActualizarPlacaConductor(ConductorYPlaca model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);
            var sql = @"UPDATE tmp.registro_fecha_despacho
                SET                               
                    Placa     = @Placa,    
                    Conductor = @Conductor
                WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket ";

            int filas = await xCon.ExecuteAsync(sql, new
            {
                model.Placa,
                model.Conductor,
                model.DocEntryHojaRuta,
                model.Linea,
                model.DocEntryTicket
            });

            return filas > 0;
        }

    }
}
