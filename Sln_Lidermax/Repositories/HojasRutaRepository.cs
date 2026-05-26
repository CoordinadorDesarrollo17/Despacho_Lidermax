using Dapper;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace Sln_Lidermax.Repositories
{
    public class HojasRutaRepository : IHojasRutaRepository
    {
        private readonly DapperContext dapperContext;

        public HojasRutaRepository(DapperContext dapperContext)
        {
            this.dapperContext = dapperContext;
        }

        private async Task<IEnumerable<HojasRutaModel>> ObtenerHojasRuta(FiltrosHojasRutaModel model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"     
                        SELECT TOP 200
                            r.DocEntry,
                            r.DocNum,
                            r.TipoRuta,
                            CONVERT(VARCHAR(10), r.TiempoPac, 103) AS TiempoPac,
                            SUM(tr.Cajas) AS Cajas,
                            r.Estado,r.Placa
                        FROM al.ORRU AS r
                        INNER JOIN al.RRU0 AS tr ON r.DocEntry = tr.DocEntry AND tr.Estado <> 'LIBERADO'
                        LEFT JOIN vt.ORTV AS tk ON tk.DocEntry = tr.DocEntryTicket 
                        WHERE r.TransDesc LIKE '%LIDERMAX%' AND 
                        CONCAT(r.DocNum,r.TipoRuta,CONVERT(VARCHAR(10), r.TiempoPac, 103),r.Estado) LIKE @Buscar
                        GROUP BY r.DocEntry,r.DocNum, r.TipoRuta, r.TiempoPac ,  r.Estado   ,r.Placa
                        ORDER BY r.Estado ASC,r.TiempoPac DESC
                    "; 

            var result = await xCon.QueryAsync<HojasRutaModel>(sql, new { Buscar = "%" + model.Buscar + "%" });

            return result;
        }

        public async Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaExcel(int docEntryHojaRuta)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @" SELECT 
                        CONVERT(VARCHAR(10), r.TiempoPac, 103) AS TiempoPac,
                        T0.Socio,
                        T5.EnvioAgencia,
                        T0.Guias,
                        T0.DocNumTicket,
                        T5.CardCode,
                        T4_1.Calle AS Calle1,
                        T4_2.Calle AS Calle2,
                        CONCAT(T4_1.Departamento, ', ', T4_1.Provincia, ', ', T4_1.Distrito) Departamento1,
                        CONCAT(T4_2.Departamento, ', ', T4_2.Provincia, ', ', T4_2.Distrito) Departamento2,
                        SUM(T3.Peso) AS Peso,
                        T0.DocEntryTicket,
                        T0.Cajas,
                        T1.NombrePer,
                        T1.DocPer,
                        T1.TelfPer,t5.agencia AS Transportista,t5.EnvioAgencia AS ModoEnvio
                    FROM al.RRU0 T0
                    LEFT OUTER JOIN al.ORRU r ON r.DocEntry = T0.DocEntry
                    LEFT OUTER JOIN vt.RTV6 T3 ON T3.DocEntry = T0.DocEntryTicket
                    LEFT OUTER JOIN vt.RTV3 T4_1 ON T4_1.DocEntry = T0.DocEntryTicket AND T4_1.IdDireccion =1
                    LEFT OUTER JOIN vt.RTV3 T4_2 ON T4_2.DocEntry = T0.DocEntryTicket AND T4_2.IdDireccion =2
                    LEFT OUTER JOIN vt.ORTV T5 ON T5.DocEntry = T0.DocEntryTicket
                    LEFT OUTER JOIN vt.RTV1 T1 ON T1.DocEntry = T0.DocEntryTicket
                    WHERE T0.DocEntry = @DocEntry AND T0.Estado <> 'LIBERADO'
                    GROUP BY 
                        T0.Socio,
                        T5.EnvioAgencia,
                        T0.Guias,            
                        T0.DocNumTicket,
                        T5.CardCode,
                        T4_1.Calle,
                        T4_1.Departamento,
                        T4_1.Provincia,
                        T4_1.Distrito,
	                    T4_2.Calle,
                        T4_2.Departamento,
                        T4_2.Provincia,
                        T4_2.Distrito,
                        T0.DocEntryTicket,
                        T0.Cajas,
                        T1.NombrePer,
                        T1.DocPer,
                        T1.TelfPer,t5.agencia,t5.EnvioAgencia, r.TiempoPac
                    ORDER BY T5.Agencia ";  

            var result = await xCon.QueryAsync<ReporteHojaRutaModel>(sql, new { DocEntry = docEntryHojaRuta });

            return result.ToList();
        }

        public async Task<IPagedList<HojasRutaModel>> ListadoHojasRutaPaginados(FiltrosHojasRutaModel model)
        {
            var result = await ObtenerHojasRuta(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<List<HojasRutaModel>> ListadoHojasRutaExcel(FiltrosHojasRutaModel model)
        {
            var result = await ObtenerHojasRuta(model);
            return result.ToList();
        }

        public async Task<List<ReporteHojaRutaModel>> ListadoTicketsPorHojasRutaPdf(int docEntryHojaRuta)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"SELECT tr.DocNumTicket,tk.CardName,
                           v1.NombrePer,
						   v1.DocPer,
						   v1.TelfPer,
						   tk.Agencia AS Transportista,
						CONCAT_WS(' - ', v3_1.Departamento, v3_1.Provincia, v3_1.Distrito, v3_1.Calle) AS Direccion1,
						CONCAT_WS(' - ', v3_2.Departamento, v3_2.Provincia, v3_2.Distrito, v3_2.Calle) AS Direccion2,
							tk.EnvioAgencia AS ModoEnvio,
                            tr.Cajas ,
                            SUM(v6.Peso) AS peso    , r.Placa                   
                            FROM al.RRU0 AS tr
                            LEFT JOIN al.ORRU AS r ON r.DocEntry = tr.DocEntry
                            LEFT JOIN vt.ORTV AS tk ON tk.DocEntry = tr.DocEntryTicket  
							LEFT JOIN vt.RTV1 AS v1 ON tk.DocEntry = v1.DocEntry 
                            LEFT JOIN vt.RTV2 AS v2 ON tk.DocEntry = v2.DocEntry AND v2.Linea =1
                            LEFT JOIN vt.RTV3 AS v3_1 ON v3_1.DocEntry = tk.DocEntry AND v3_1.IdDireccion =1
                            LEFT JOIN vt.RTV3 AS v3_2 ON v3_2.DocEntry = tk.DocEntry AND v3_2.IdDireccion =2
                            LEFT JOIN vt.RTV6 AS v6 ON v6.DocEntry = tk.DocEntry 
                            WHERE tr.DocEntry =  @DocEntry AND tr.Estado <> 'LIBERADO'
                            GROUP BY
                            tr.DocNumTicket,tk.CardName,
                            v3_1.Departamento ,
                            v3_1.Provincia ,
                            v3_1.Distrito ,
                            v3_2.Departamento ,
                            v3_2.Provincia ,
                            v3_2.Distrito ,                       
                            v3_1.Calle ,
                            v3_2.Calle ,
                            tk.Agencia ,
                            tk.EnvioAgencia ,
                            tr.Cajas,
							v1.NombrePer,
						    v1.DocPer,
						    v1.TelfPer , r.Placa  
                            ORDER BY tk.Agencia ";


            var result = await xCon.QueryAsync<ReporteHojaRutaModel>(sql, new { DocEntry = docEntryHojaRuta });

            return result.ToList();

        }

    }
}
