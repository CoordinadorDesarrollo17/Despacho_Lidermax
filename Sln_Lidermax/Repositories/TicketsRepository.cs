using Dapper;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Dtos;
using X.PagedList;
using X.PagedList.Extensions;
using DocumentFormat.OpenXml.EMMA;

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
        private async Task<IEnumerable<TicketsDto>> ObtenerTicketsRecogidos(FiltrosTicketsDto model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"     
                        SELECT TOP 200 tr.DocEntry AS DocEntryHojaRuta, tk.DocEntry AS DocEntryTicket, tk.DocNum AS DocNumTicket,
                               tk.CardCode, tk.CardName,
                               (v3_1.Calle + ' / ' + v3_1.Distrito + ' - ' + v3_1.Provincia + ' - ' + v3_1.Departamento ) AS Direccion1,
                               CASE WHEN tk.EnvioAgencia LIKE '%Agencia%' THEN (v3_2.Calle + ' / ' +  v3_2.Distrito + ' - ' + v3_2.Provincia + ' - ' + v3_2.Departamento) ELSE '' END AS Direccion2,
                               tk.Agencia, tk.EnvioAgencia AS ModoEnvio, tk.Cajas, SUM(v6.Peso) AS Peso,
                               rfd.FechaRecojo, rfd.FechaDespacho, rfd.Estado, v1.NombrePer AS Contacto, v1.TelfPer AS Telefono,
                               tk.DistritoEnvio AS DistritoTransporte, RIGHT(tr.Guias,13) AS GuiaRemision, rfd.GuiaTransportista, rfd.FechaDevolucion, rfd.FechaEntrega
                        FROM al.RRU0 AS tr 
                        LEFT JOIN al.ORRU AS r ON r.DocEntry = tr.DocEntry 
                        LEFT JOIN vt.ORTV AS tk ON tr.DocEntryTicket = tk.DocEntry 
                        LEFT JOIN vt.RTV1 AS v1 ON v1.DocEntry = tk.DocEntry
                        LEFT JOIN vt.RTV3 AS v3_1 ON v3_1.DocEntry = tk.DocEntry AND v3_1.IdDireccion = 1
                        LEFT JOIN vt.RTV3 AS v3_2 ON v3_2.DocEntry = tk.DocEntry AND v3_2.IdDireccion = 2
                        LEFT JOIN vt.RTV6 AS v6 ON v6.DocEntry = tk.DocEntry 
                        LEFT JOIN tmp.registro_fecha_despacho AS rfd ON rfd.DocEntryTicket = tk.DocEntry 
                        WHERE r.TransDesc LIKE '%LIDERMAX%'
                          AND tr.Estado <> 'LIBERADO' 
                          AND rfd.Estado <> '' --IN ('RECOGIDO','ENVIADO') 
                          AND CONCAT(CONVERT(VARCHAR(10), rfd.FechaRecojo, 103),CONVERT(VARCHAR(10), rfd.FechaDespacho, 103),rfd.Estado,tk.DistritoEnvio,tr.Guias,v1.TelfPer,v1.NombrePer,tk.DocNum,tk.CardCode,tk.CardName,v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,v3_1.Calle,v3_2.Departamento, v3_2.Provincia,v3_2.Distrito,tk.Agencia,tk.EnvioAgencia) LIKE @Buscar
                        AND (@DocEntry IS NULL OR tk.DocEntry = @DocEntry)
                        AND (@Estado IS NULL OR rfd.Estado = @Estado)
                        AND (@FechaDespacho IS NULL OR CAST(rfd.FechaDespacho AS DATE) = CAST(@FechaDespacho AS DATE))
                        GROUP BY tr.DocEntry,tk.DocEntry,tk.DocNum,tk.CardCode,tk.CardName,
                                 v3_1.Calle,v3_2.Calle, tk.Agencia,tk.EnvioAgencia, tk.Cajas,
                                 rfd.FechaRecojo,rfd.FechaDespacho,rfd.Estado, v1.NombrePer,v1.TelfPer,
                                 v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,tk.DistritoEnvio,
                                 v3_2.Departamento,v3_2.Provincia,v3_2.Distrito,tr.Guias,rfd.GuiaTransportista, rfd.FechaDevolucion,rfd.FechaEntrega
                        ORDER BY rfd.Estado DESC
                    "; // tk.EnvioAgencia IN ('Agencia de transporte','Domicilio del Cliente') 

            var result = await xCon.QueryAsync<TicketsDto>(sql, new { Buscar = "%" + model.Buscar + "%", DocEntry = model.DocEntryTicket, Estado = model.Estado, FechaDespacho = model.FechaDespacho });
            return result;
        }

        private async Task<IEnumerable<TicketsDto>> ObtenerTicketsPorHojaRuta(FiltrosTicketsDto model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"     
                        SELECT TOP 200 tr.DocEntry AS DocEntryHojaRuta, tk.DocEntry AS DocEntryTicket, tk.DocNum AS DocNumTicket,
                               tk.CardCode, tk.CardName,
                               (v3_1.Calle + ' / ' + v3_1.Distrito + ' - ' + v3_1.Provincia + ' - ' + v3_1.Departamento) AS Direccion1,
                               CASE WHEN tk.EnvioAgencia LIKE '%Agencia%' THEN (v3_2.Calle + ' / ' + v3_2.Distrito + ' - ' + v3_2.Provincia + ' - ' + v3_2.Departamento) ELSE '' END AS Direccion2,
                               tk.Agencia, tk.EnvioAgencia AS ModoEnvio, tk.Cajas, SUM(v6.Peso) AS Peso,
                               rfd.Estado, v1.NombrePer AS Contacto, v1.TelfPer AS Telefono,
                               tk.DistritoEnvio AS DistritoTransporte, RIGHT(tr.Guias,13) AS GuiaRemision
                        FROM al.RRU0 AS tr 
                        LEFT JOIN vt.ORTV AS tk ON tr.DocEntryTicket = tk.DocEntry 
                        LEFT JOIN vt.RTV1 AS v1 ON v1.DocEntry = tk.DocEntry
                        LEFT JOIN vt.RTV3 AS v3_1 ON v3_1.DocEntry = tk.DocEntry AND v3_1.IdDireccion = 1
                        LEFT JOIN vt.RTV3 AS v3_2 ON v3_2.DocEntry = tk.DocEntry AND v3_2.IdDireccion = 2
                        LEFT JOIN vt.RTV6 AS v6 ON v6.DocEntry = tk.DocEntry 
                        LEFT JOIN tmp.registro_fecha_despacho AS rfd ON rfd.DocEntryTicket = tk.DocEntry 
                        WHERE tr.DocEntry = @DocEntry AND tr.Estado <> 'LIBERADO' 
                          AND CONCAT(RIGHT(tr.Guias,13),tk.DistritoEnvio,v1.TelfPer,tk.DocNum,tk.CardCode,tk.CardName,v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,v3_1.Calle,v3_2.Departamento,v3_2.Provincia,v3_2.Distrito,tk.Agencia,tk.EnvioAgencia,rfd.Estado,v1.NombrePer) LIKE @Buscar
                        GROUP BY tr.DocEntry,tk.DocEntry,tk.DocNum,tk.CardCode,tk.CardName,
                                 v3_1.Calle,v3_2.Calle, tk.Agencia,tk.EnvioAgencia, tk.Cajas,
                                 rfd.Estado,rfd.DocEntryTicket, v1.NombrePer,v1.TelfPer,
                                 v3_1.Departamento,v3_1.Provincia,v3_1.Distrito,
                                 v3_2.Departamento,v3_2.Provincia,v3_2.Distrito,tk.DistritoEnvio,tr.Guias
                        ORDER BY CASE WHEN rfd.DocEntryTicket IS NULL THEN 0 ELSE 1 END, tk.DocNum
                     ";

            var result = await xCon.QueryAsync<TicketsDto>(sql, new { Buscar = "%" + model.Buscar + "%", DocEntry = model.DocEntryHojaRuta });
            return result;
        }

        public async Task<IPagedList<TicketsDto>> ListadoTicketsRecogidos(FiltrosTicketsDto model)
        {
            var result = await ObtenerTicketsRecogidos(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<List<TicketsDto>> ListadoTicketsRecogidosExcel(FiltrosTicketsDto model)
        {
            var result = await ObtenerTicketsRecogidos(model);
            return result.ToList();
        }

        public async Task<IPagedList<TicketsDto>> ListadoTickets(FiltrosTicketsDto model)
        {
            var result = await ObtenerTicketsPorHojaRuta(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<List<TicketsDto>> ListadoTicketsExcel(FiltrosTicketsDto model)
        {
            var result = await ObtenerTicketsPorHojaRuta(model);
            return result.ToList();
        }

        public async Task<bool> InsertarTicketsRecogidos(int docEntryTicket,SqlConnection con,SqlTransaction tx)
        {
            var sql = @"
                        INSERT INTO [tmp].[registro_fecha_despacho]
                        (DocEntryTicket, FechaRecojo, FechaDespacho, Estado)
                        VALUES (@DocEntryTicket, GETDATE(), NULL, 'RECOGIDO')";

            var result = await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);

            sql = "UPDATE [al].[RRU0] SET Estado ='RECOGIDO' WHERE DocEntryTicket = @DocEntryTicket ";
            var result1 = await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='RECOGIDO' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);

            return result > 0 && result1 > 0 && result2 > 0;
        }

        public async Task<bool> ActualizarFechaDespacho(TicketsDto model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                    SET FechaDespacho = @FechaDespacho
                    WHERE DocEntryTicket = @DocEntryTicket";

            var result = await xCon.ExecuteAsync(sql, new
            {
                 model.DocEntryTicket,
                 model.FechaDespacho
            });

            return result > 0;
        }
        public async Task<bool> ActualizarGuiaTransportista(TicketsDto model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                    SET GuiaTransportista = @GuiaTransportista
                    WHERE DocEntryTicket = @DocEntryTicket";

            var result = await xCon.ExecuteAsync(sql, new
            {
                model.DocEntryTicket,
                model.GuiaTransportista
            });

            return result > 0;
        }
        public async Task<bool> ActualizarEstadoEnviado(int docEntryTicket, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                SET Estado = 'ENVIADO'
                WHERE DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);


            sql = "UPDATE [al].[RRU0] SET Estado ='ENVIADO' WHERE DocEntryTicket = @DocEntryTicket ";
           var result1 =  await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='ENVIADO' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = docEntryTicket }, tx);

            return result > 0 && result1 > 0 && result2 > 0;
        }
        public async Task<bool> ActualizarEstadoEntregado(TicketSeleccionadoDto model, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"UPDATE [tmp].[registro_fecha_despacho]
                SET Estado = 'ENTREGADO', FechaEntrega = @FechaEntrega
                WHERE DocEntryTicket = @DocEntryTicket";

            var result = await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket, FechaEntrega = model.Fecha }, tx);

            sql = "UPDATE [al].[RRU0] SET Estado ='ENTREGADO' WHERE DocEntryTicket = @DocEntryTicket ";
            var result1 = await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='ENTREGADO' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket }, tx);

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
                        ON rt.DocEntryTicket = rfd.DocEntryTicket AND rfd.Estado IN @Estado
                    WHERE rt.DocEntry = @DocEntryHojaRuta";

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

        public async Task<bool> DevolverTicket(TicketSeleccionadoDto model, SqlConnection con, SqlTransaction tx)
        {
            var sql = @"UPDATE [al].[RRU0] SET Estado ='DEVOLUCION' WHERE DocEntryTicket = @DocEntryTicket";
            var result1 =  await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket }, tx);

            sql = "UPDATE [vt].[ORTV] SET Estado ='DEVOLUCION' WHERE DocEntry = @DocEntryTicket ";
            var result2 = await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket }, tx);

            sql = "UPDATE [tmp].[registro_fecha_despacho] SET Estado ='DEVOLUCION', FechaDevolucion = @FechaDevolucion WHERE DocEntryTicket = @DocEntryTicket ";
            var result3 = await con.ExecuteAsync(sql, new { DocEntryTicket = model.DocEntryTicket, FechaDevolucion = model.Fecha}, tx);

            return result1 > 0 && result2 > 0 && result3>0;
        }






    }
}
