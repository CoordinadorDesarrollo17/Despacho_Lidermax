using Dapper;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Sap.Data.Hana;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace Sln_Lidermax.Repositories
{
    public class TicketsRepository : ITicketsRepository
    {
        private readonly DapperContext dapperContext;
        public TicketsRepository(DapperContext dapperContext)
        {
            this.dapperContext = dapperContext;
        }

        // Método privado para Tickets Recogidos
        private async Task<IEnumerable<TicketsModel>> ObtenerTicketsRecogidos(FiltrosTicketsModel model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"     
                        SELECT TOP 200 tr.DocEntry AS DocEntryHojaRuta,tr.Linea, tk.DocEntry AS DocEntryTicket, tk.DocNum AS DocNumTicket,
                               tk.CardCode, tk.CardName,
                               (v3_1.Calle + ' / ' + v3_1.Distrito + ' - ' + v3_1.Provincia + ' - ' + v3_1.Departamento ) AS Direccion1,
                               CASE 
                                    WHEN tk.EnvioAgencia LIKE '%Agencia%' THEN 
                                        (CASE 
                                            WHEN v3_2.Calle IS NULL OR v3_2.Calle = '' 
                                                THEN v3_2.Distrito + ' - ' + v3_2.Provincia + ' - ' + v3_2.Departamento
                                            ELSE v3_2.Calle + ' / ' + v3_2.Distrito + ' - ' + v3_2.Provincia + ' - ' + v3_2.Departamento
                                         END)
                                    ELSE '' 
                                END AS Direccion2,
                               tk.Agencia, tk.EnvioAgencia AS ModoEnvio, tk.Cajas, SUM(v6.Peso) AS Peso,
                               rfd.FechaRecojo, rfd.FechaDespacho, rfd.Estado, v1.NombrePer AS Contacto, v1.TelfPer AS Telefono,
                               tk.DistritoEnvio AS DistritoTransporte, tr.Guias AS GuiaRemision, rfd.GuiaTransportista, rfd.FechaDevolucion, rfd.FechaEntrega, rfd.Observacion , rfd.Excluido , rfd.MontoFlete, rfd.EstadoPago, rfd.IdRol , r.Placa , rfd.Factura , rfd.GuiaTransportista
                        FROM al.RRU0 AS tr 
                        LEFT JOIN al.ORRU AS r ON r.DocEntry = tr.DocEntry 
                        LEFT JOIN vt.ORTV AS tk ON tr.DocEntryTicket = tk.DocEntry 
                        LEFT JOIN vt.RTV1 AS v1 ON v1.DocEntry = tk.DocEntry
                        LEFT JOIN vt.RTV3 AS v3_1 ON v3_1.DocEntry = tk.DocEntry AND v3_1.IdDireccion = 1
                        LEFT JOIN vt.RTV3 AS v3_2 ON v3_2.DocEntry = tk.DocEntry AND v3_2.IdDireccion = 2
                        LEFT JOIN vt.RTV6 AS v6 ON v6.DocEntry = tk.DocEntry 
                        LEFT JOIN tmp.registro_fecha_despacho AS rfd ON rfd.DocEntryHojaRuta=tr.DocEntry AND rfd.Linea= tr.Linea AND rfd.DocEntryTicket = tr.DocEntryTicket
                        WHERE ( r.TipoRuta = 'VG' AND r.TransDesc LIKE '%LIDERMAX%' )
                          AND tr.Estado <> 'LIBERADO' 
                          AND rfd.Estado <> '' --IN ('RECOGIDO','ENVIADO') 
                          AND CONCAT(CONVERT(VARCHAR(10), rfd.FechaRecojo, 103),CONVERT(VARCHAR(10), rfd.FechaEntrega, 103),rfd.Estado,tk.DistritoEnvio,tr.Guias,v1.TelfPer,v1.NombrePer,tk.DocNum,tk.CardCode,tk.CardName,v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,v3_1.Calle,v3_2.Departamento, v3_2.Provincia,v3_2.Distrito,tk.Agencia,tk.EnvioAgencia,rfd.EstadoPago) LIKE @Buscar
                        AND (@DocEntry IS NULL OR tk.DocEntry = @DocEntry)
                        AND (@Estado IS NULL OR rfd.Estado = @Estado)
                        AND (@FechaEntrega IS NULL OR CAST(rfd.FechaEntrega AS DATE) = CAST(@FechaEntrega AS DATE))
                        AND (@FechaRecojo IS NULL OR CAST(rfd.FechaRecojo AS DATE) = CAST(@FechaRecojo AS DATE))
                        AND (@DocNumHojaRuta IS NULL OR r.DocNum = @DocNumHojaRuta)
                        GROUP BY tr.DocEntry,tk.DocEntry,tk.DocNum,tk.CardCode,tk.CardName,
                                 v3_1.Calle,v3_2.Calle, tk.Agencia,tk.EnvioAgencia, tk.Cajas,
                                 rfd.FechaRecojo,rfd.FechaDespacho,rfd.Estado, v1.NombrePer,v1.TelfPer,
                                 v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,tk.DistritoEnvio,
                                 v3_2.Departamento,v3_2.Provincia,v3_2.Distrito,tr.Guias,rfd.GuiaTransportista, rfd.FechaDevolucion,rfd.FechaEntrega,tr.Linea, rfd.Observacion , rfd.Excluido , rfd.MontoFlete, rfd.EstadoPago,  rfd.IdRol , r.Placa , rfd.Factura, rfd.GuiaTransportista
                        ORDER BY FechaRecojo DESC
                    "; // tk.EnvioAgencia IN ('Agencia de transporte','Domicilio del Cliente') 

            var result = await xCon.QueryAsync<TicketsModel>(sql, new { Buscar = "%" + model.Buscar + "%", DocEntry = model.DocEntryTicket, Estado = model.Estado, FechaEntrega = model.FechaEntrega, DocNumHojaRuta = model.DocNumHojaRuta , FechaRecojo = model.FechaRecojo });
            return result;
        }

        private async Task<IEnumerable<TicketsModel>> ObtenerTicketsPorHojaRuta(FiltrosTicketsModel model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"     
                        SELECT TOP 200 tr.DocEntry AS DocEntryHojaRuta,tr.Linea, tk.DocEntry AS DocEntryTicket, tk.DocNum AS DocNumTicket,
                               tk.CardCode, tk.CardName,
                               (v3_1.Calle + ' / ' + v3_1.Distrito + ' - ' + v3_1.Provincia + ' - ' + v3_1.Departamento) AS Direccion1,
                                 CASE 
                                    WHEN tk.EnvioAgencia LIKE '%Agencia%' THEN 
                                        (CASE 
                                            WHEN v3_2.Calle IS NULL OR v3_2.Calle = '' 
                                                THEN v3_2.Distrito + ' - ' + v3_2.Provincia + ' - ' + v3_2.Departamento
                                            ELSE v3_2.Calle + ' / ' + v3_2.Distrito + ' - ' + v3_2.Provincia + ' - ' + v3_2.Departamento
                                         END)
                                    ELSE '' 
                                END AS Direccion2,
                               tk.Agencia, tk.EnvioAgencia AS ModoEnvio, tk.Cajas, SUM(v6.Peso) AS Peso,
                               rfd.Estado, v1.NombrePer AS Contacto, v1.TelfPer AS Telefono,
                               tk.DistritoEnvio AS DistritoTransporte, tr.Guias AS GuiaRemision, tk.EntregaPedido, rfd.Placa, rfd.Factura
                        FROM al.RRU0 AS tr 
                        LEFT JOIN al.ORRU AS r ON r.DocEntry = tr.DocEntry 
                        LEFT JOIN vt.ORTV AS tk ON tr.DocEntryTicket = tk.DocEntry 
                        LEFT JOIN vt.RTV1 AS v1 ON v1.DocEntry = tk.DocEntry
                        LEFT JOIN vt.RTV3 AS v3_1 ON v3_1.DocEntry = tk.DocEntry AND v3_1.IdDireccion = 1
                        LEFT JOIN vt.RTV3 AS v3_2 ON v3_2.DocEntry = tk.DocEntry AND v3_2.IdDireccion = 2
                        LEFT JOIN vt.RTV6 AS v6 ON v6.DocEntry = tk.DocEntry 
                        LEFT JOIN tmp.registro_fecha_despacho AS rfd ON rfd.DocEntryHojaRuta=tr.DocEntry AND rfd.Linea= tr.Linea AND rfd.DocEntryTicket = tr.DocEntryTicket
                        WHERE tr.DocEntry = @DocEntry AND (  (r.TipoRuta = 'VG' AND r.TransDesc LIKE '%LIDERMAX%') OR (r.TipoRuta='VD' AND tk.LugarDestino = 'DOMICILIO' AND tk.EntregaPedido = 'PROVINCIA') )
                          AND tr.Estado <> 'LIBERADO' 
                          AND CONCAT(RIGHT(tr.Guias,13),tk.DistritoEnvio,v1.TelfPer,tk.DocNum,tk.CardCode,tk.CardName,v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,v3_1.Calle,v3_2.Departamento,v3_2.Provincia,v3_2.Distrito,tk.Agencia,tk.EnvioAgencia,rfd.Estado,v1.NombrePer) LIKE @Buscar
                        GROUP BY tr.DocEntry,tk.DocEntry,tk.DocNum,tk.CardCode,tk.CardName,
                                 v3_1.Calle,v3_2.Calle, tk.Agencia,tk.EnvioAgencia, tk.Cajas,
                                 rfd.Estado,rfd.DocEntryTicket, v1.NombrePer,v1.TelfPer,
                                 v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,
                                 v3_2.Departamento,v3_2.Provincia,v3_2.Distrito,tk.DistritoEnvio,tr.Guias,tr.Linea , tk.EntregaPedido , rfd.Placa, rfd.Factura
                        ORDER BY tk.Agencia
                     ";

            var result = await xCon.QueryAsync<TicketsModel>(sql, new { Buscar = "%" + model.Buscar + "%", DocEntry = model.DocEntryHojaRuta });
            return result;
        }

        public async Task<IPagedList<TicketsModel>> ListadoTicketsRecogidos(FiltrosTicketsModel model)
        {
            var result = await ObtenerTicketsRecogidos(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<List<TicketsModel>> ListadoTicketsRecogidosExcel(FiltrosTicketsModel model)
        {
            var result = await ObtenerTicketsRecogidos(model);
            return result.ToList();
        }

        public async Task<IPagedList<TicketsModel>> ListadoTickets(FiltrosTicketsModel model)
        {
            var result = await ObtenerTicketsPorHojaRuta(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<List<TicketsModel>> ListadoTicketsExcel(FiltrosTicketsModel model)
        {
            var result = await ObtenerTicketsPorHojaRuta(model);
            return result.ToList();
        }

        public async Task<string> ObtenerEstadoTicket(int docEntryHojaRuta, int linea, int docEntryTicket, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"SELECT Estado 
                FROM [al].[RRU0] 
                WHERE DocEntry = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            return await con.QueryFirstOrDefaultAsync<string>(sql, new { DocEntryHojaRuta = docEntryHojaRuta, Linea = linea, DocEntryTicket = docEntryTicket }, tx);
        }

        public async Task<bool> InsertarTicketsRecogidos(int docEntryHojaRuta,int linea, int docEntryTicket,SqlConnection con,SqlTransaction tx)
        {
            var sql = @"
                        INSERT INTO [tmp].[registro_fecha_despacho]
                        (DocEntryHojaRuta,Linea,DocEntryTicket, FechaRecojo, FechaDespacho, Estado, Excluido)
                        VALUES (@DocEntryHojaRuta,@Linea,@DocEntryTicket, GETDATE(), NULL, 'RECOGIDO',0)";

            var result = await con.ExecuteAsync(sql, new {  DocEntryHojaRuta = docEntryHojaRuta , Linea= linea,DocEntryTicket = docEntryTicket }, tx);

            sql = "UPDATE [al].[RRU0] SET Estado ='RECOGIDO' WHERE DocEntry = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket ";
            var result1 = await con.ExecuteAsync(sql, new { DocEntryHojaRuta = docEntryHojaRuta, Linea= linea, DocEntryTicket = docEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='RECOGIDO' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);

            return result > 0 && result1 > 0 && result2 > 0;
        }
        public async Task<bool> ActualizarGuiaTransportista(TicketsModel model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                    SET GuiaTransportista = @GuiaTransportista
                    WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await xCon.ExecuteAsync(sql, new
            {
                model.DocEntryHojaRuta,
                model.Linea,
                model.DocEntryTicket,
                model.GuiaTransportista
            });

            return result > 0;
        }
        public async Task<bool> ActualizarObservacion(TicketSeleccionadoDto model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                    SET Observacion = @Observacion
                    WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await xCon.ExecuteAsync(sql, new
            {
                model.DocEntryHojaRuta,
                model.Linea,
                model.DocEntryTicket,
                model.Observacion
            });

            return result > 0;
        }
        public async Task<bool> ActualizarFechaDespacho(TicketsModel model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                    SET FechaDespacho = @FechaDespacho
                    WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await xCon.ExecuteAsync(sql, new
            {
                 model.DocEntryHojaRuta,
                 model.Linea,
                 model.DocEntryTicket,
                 model.FechaDespacho
            });

            return result > 0;
        }
      
        public async Task<bool> ActualizarEstadoEnviado(int docEntryHojaRuta,int linea, int docEntryTicket, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                SET Estado = 'ENVIADO'
                WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new { DocEntryHojaRuta= docEntryHojaRuta, Linea=linea, DocEntryTicket = docEntryTicket }, tx);

            sql = "UPDATE [al].[RRU0] SET Estado ='ENVIADO' WHERE DocEntry = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket ";
           var result1 =  await con.ExecuteAsync(sql, new { DocEntryHojaRuta = docEntryHojaRuta, Linea = linea, DocEntryTicket = docEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='ENVIADO' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);

            return result > 0 && result1 > 0 && result2 > 0;
        }   
        public async Task<(int TotalTickets, int TicketsObtenidos)> ObtenerConteoTickets(int docEntryHojaRuta, string[] estado, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"
                    SELECT 
                        COUNT(*) AS TotalTickets,
                        COUNT(rfd.DocEntryTicket) AS TicketsEnviados
                    FROM [al].[RRU0] rt
                    LEFT JOIN [tmp].[registro_fecha_despacho] rfd 
                        ON rfd.DocEntryHojaRuta = rt.DocEntry AND rfd.Linea = rt.Linea AND rfd.DocEntryTicket = rt.DocEntryTicket AND rfd.Estado IN @Estado
                    WHERE rt.DocEntry = @DocEntryHojaRuta AND rt.Estado<>'LIBERADO' ";

            return await con.QueryFirstAsync<(int, int)>(sql,new { DocEntryHojaRuta = docEntryHojaRuta, Estado = estado },tx);
        }

        public async Task<bool> ActualizarEstadoHojaRuta(int docEntryHojaRuta, string estado, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"UPDATE [al].[ORRU]
                SET Estado = @Estado
                WHERE DocEntry = @DocEntryHojaRuta";

            var result = await con.ExecuteAsync(sql,
                new { DocEntryHojaRuta = docEntryHojaRuta, Estado = estado }, tx);

            return result > 0;
        }
        public async Task<bool> ActualizarEstadoEntregado(TicketSeleccionadoDto model, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                SET Estado = 'ENTREGADO', FechaEntrega = @FechaEntrega, IdRol = @IdRol
                WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new { DocEntryHojaRuta = model.DocEntryHojaRuta, Linea = model.Linea, DocEntryTicket = model.DocEntryTicket, FechaEntrega = model.Fecha, IdRol = model.IdRol }, tx);

            sql = "UPDATE [al].[RRU0] SET Estado ='ENTREGADO' WHERE DocEntry = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket ";
            var result1 = await con.ExecuteAsync(sql, new { DocEntryHojaRuta = model.DocEntryHojaRuta, Linea = model.Linea, DocEntryTicket = model.DocEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='ENTREGADO' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket }, tx);

            return result > 0 && result1 > 0 && result2 > 0;
        }
        public async Task<bool> DevolverTicket(TicketSeleccionadoDto model, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"UPDATE [al].[RRU0] SET Estado ='DEVOLUCION' WHERE DocEntry = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";
            var result1 =  await con.ExecuteAsync(sql, new { DocEntryHojaRuta = model.DocEntryHojaRuta, Linea = model.Linea, DocEntryTicket = model.DocEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='DEVOLUCION' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket }, tx);

            sql = "UPDATE [tmp].[registro_fecha_despacho] SET Estado ='DEVOLUCION', FechaDevolucion = @FechaDevolucion WHERE  DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket ";
            var result3 = await con.ExecuteAsync(sql, new { DocEntryHojaRuta = model.DocEntryHojaRuta, Linea = model.Linea, DocEntryTicket = model.DocEntryTicket, FechaDevolucion = model.Fecha}, tx);

            return result1 > 0 && result2 > 0 && result3>0;
        }


        //Excluir ticket
        public async Task<bool> ExcluirTicket(TicketSeleccionadoDto model, SqlConnection con , SqlTransaction tx )
        {
          
            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                    SET Excluido = @Excluido
                    WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new
            {
                model.DocEntryHojaRuta,
                model.Linea,
                model.DocEntryTicket,
                model.Excluido
            }, tx);

            return result > 0;
        }

        public async Task<TicketsModel> ObtenerTicket(int docEntryHojaRuta, int linea, int docEntryTicket, SqlConnection con,SqlTransaction tx) 
        {
            string query = @"
        SELECT EstadoPago,MontoFlete,IdRol,Estado
        FROM tmp.registro_fecha_despacho
      WHERE DocEntryHojaRuta = @DocEntryHojaRuta AND Linea = @Linea AND DocEntryTicket = @DocEntryTicket ";

            return await con.QueryFirstOrDefaultAsync<TicketsModel>(
                query,
                new
                {
                    DocEntryHojaRuta = docEntryHojaRuta,
                    Linea = linea,
                    DocEntryTicket = docEntryTicket
                },
                tx
            );
        }


        public async Task<List<DetalleTicketModel>> obtenerDet2Ticket(int DocEntry)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);
            string query = "SELECT DocEntry, Linea, NroSap FROM vt.RTV2 WHERE DocEntry = @DocEntry ORDER BY Linea";
            try
            {
                return (await xCon.QueryAsync<DetalleTicketModel>(query, new { DocEntry })).ToList();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<string>> ObtenerFacturasxDocEntry(int NroSap)
        {
            using var xCon = new HanaConnection(dapperContext.hanaConnectionString);
            try
            {
                // 1. Obtener DocEntry de la orden
                string queryOrden = "SELECT \"DocEntry\" FROM \"ORDR\" WHERE \"DocNum\" = :NroSap";
                var docEntryOrden = await xCon.QueryFirstOrDefaultAsync<int?>(queryOrden, new { NroSap });

                if (docEntryOrden == null)
                    return new List<string>();

                int DocEntryOrden = docEntryOrden.Value;

                // 2. Query 1
                string query1 = $@"
        SELECT T4.""NumAtCard""
        FROM ODLN T0
        INNER JOIN DLN1 T1 ON T1.""DocEntry"" = T0.""DocEntry""
        INNER JOIN RDR1 T2 ON T2.""DocEntry"" = T1.""BaseEntry"" 
            AND T2.""ObjType"" = T1.""BaseType""
            AND T2.""LineNum"" = T1.""BaseLine"" 
            AND T2.""DocEntry"" = {DocEntryOrden}
        INNER JOIN INV1 T3 ON T3.""BaseEntry"" = T1.""DocEntry"" 
            AND T3.""BaseType"" = T1.""ObjType""
            AND T3.""BaseLine"" = T1.""LineNum""
        INNER JOIN OINV T4 ON T4.""DocEntry"" = T3.""DocEntry"" 
            AND T4.""CANCELED"" = 'N' 
        WHERE T0.""CANCELED"" = 'N'
            AND T4.""DocEntry"" NOT IN (
                SELECT DISTINCT R1.""BaseEntry""
                FROM RIN1 R1
                INNER JOIN ORIN R0 ON R0.""DocEntry"" = R1.""DocEntry""
                WHERE R0.""CANCELED"" = 'N'
                    AND R1.""BaseType"" = '13'
            )
        GROUP BY T4.""NumAtCard""";

                var resultado = (await xCon.QueryAsync<string>(query1)).ToList();

                if (resultado.Count > 0)
                    return resultado;

                // 3. Query 2: 
                string query2 = $@"
        SELECT T0.""NumAtCard""
        FROM OINV T0
        INNER JOIN INV1 T1 ON T1.""DocEntry"" = T0.""DocEntry""
        INNER JOIN RDR1 T2 ON T2.""DocEntry"" = T1.""BaseEntry"" 
            AND T2.""ObjType"" = T1.""BaseType""
            AND T2.""LineNum"" = T1.""BaseLine"" 
            AND T2.""DocEntry"" = {DocEntryOrden}
        WHERE T0.""CANCELED"" = 'N' 
        GROUP BY T0.""NumAtCard""";

                return (await xCon.QueryAsync<string>(query2)).ToList();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}
