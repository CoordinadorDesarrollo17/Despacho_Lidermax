using Dapper;
using Microsoft.Data.SqlClient;
using Sap.Data.Hana;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;
using System;
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
                            r.Estado,r.Placa,
                            CONVERT(VARCHAR(5), r.TiempoPac, 108) AS HoraPac , r.Estado2
                        FROM al.ORRU AS r
                        INNER JOIN al.RRU0 AS tr ON r.DocEntry = tr.DocEntry AND tr.Estado <> 'LIBERADO'
                        LEFT JOIN vt.ORTV AS tk ON tk.DocEntry = tr.DocEntryTicket 
                        WHERE ( (r.TipoRuta = 'VG' AND r.TransDesc LIKE '%LIDERMAX%') OR (r.TipoRuta='VD' AND tk.LugarDestino = 'DOMICILIO' AND tk.EntregaPedido IN ('PROVINCIA','RECOJO') ) )
                        AND CONCAT(r.DocNum,r.TipoRuta,CONVERT(VARCHAR(10), r.TiempoPac, 103),r.Estado) LIKE @Buscar
                        GROUP BY r.DocEntry,r.DocNum, r.TipoRuta, r.TiempoPac ,  r.Estado   ,r.Placa , r.Estado2
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
                        CASE WHEN ISNULL(SUM(T3.Peso),0) = 0 THEN T0.Peso ELSE ISNULL(SUM(T3.Peso),0) END  AS peso    , 
                        T0.DocEntryTicket,
                        T0.Cajas,
                        T1.NombrePer,
                        T1.DocPer,
                        T1.TelfPer,t5.agencia AS Transportista,t5.EnvioAgencia AS ModoEnvio     , r.Estado2             
                    FROM al.RRU0 T0
                    LEFT JOIN al.ORRU r ON r.DocEntry = T0.DocEntry
                    LEFT JOIN vt.RTV6 T3 ON T3.DocEntry = T0.DocEntryTicket
                    LEFT JOIN vt.RTV3 T4_1 ON T4_1.DocEntry = T0.DocEntryTicket AND T4_1.IdDireccion =1
                    LEFT JOIN vt.RTV3 T4_2 ON T4_2.DocEntry = T0.DocEntryTicket AND T4_2.IdDireccion =2
                    LEFT JOIN vt.ORTV T5 ON T5.DocEntry = T0.DocEntryTicket
                    LEFT JOIN vt.RTV1 T1 ON T1.DocEntry = T0.DocEntryTicket     
                    WHERE T0.DocEntry = @DocEntry AND (  (r.TipoRuta = 'VG' AND r.TransDesc LIKE '%LIDERMAX%') OR (r.TipoRuta='VD' AND T5.LugarDestino = 'DOMICILIO' AND T5.EntregaPedido IN ('PROVINCIA','RECOJO') ) )
                    AND T0.Estado <> 'LIBERADO'
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
                        T1.TelfPer,t5.agencia,t5.EnvioAgencia, r.TiempoPac  , r.Estado2  ,T0.Peso
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

            var sql = @"SELECT 
                           CASE WHEN ISNULL(tr.DocNumTicket,'') = '' THEN tr.Guias ELSE CAST(tr.DocNumTicket AS NVARCHAR(20)) END AS DocNumTicket,
                           tk.CardName,
                           v1.NombrePer,
						   v1.DocPer,
						   v1.TelfPer,
						   tk.Agencia AS Transportista,
						CONCAT_WS(' - ', v3_1.Departamento, v3_1.Provincia, v3_1.Distrito, v3_1.Calle) AS Direccion1,
						CONCAT_WS(' - ', v3_2.Departamento, v3_2.Provincia, v3_2.Distrito, v3_2.Calle) AS Direccion2,
							tk.EnvioAgencia AS ModoEnvio,
                            tr.Cajas ,
                            CASE WHEN ISNULL(SUM(v6.Peso),0) = 0 THEN tr.Peso ELSE ISNULL(SUM(v6.Peso),0) END  AS peso    , 
                            r.Placa       , tk.DetallePedido   , r.TipoRuta   , r.Trans2Desc AS Conductor   , r.Estado2 , tr.Guias
                            FROM al.RRU0 AS tr
                            LEFT JOIN al.ORRU AS r ON r.DocEntry = tr.DocEntry
                            LEFT JOIN vt.ORTV AS tk ON tk.DocEntry = tr.DocEntryTicket  
							LEFT JOIN vt.RTV1 AS v1 ON tk.DocEntry = v1.DocEntry 
                            LEFT JOIN vt.RTV2 AS v2 ON tk.DocEntry = v2.DocEntry AND v2.Linea =1
                            LEFT JOIN vt.RTV3 AS v3_1 ON v3_1.DocEntry = tk.DocEntry AND v3_1.IdDireccion =1
                            LEFT JOIN vt.RTV3 AS v3_2 ON v3_2.DocEntry = tk.DocEntry AND v3_2.IdDireccion =2
                            LEFT JOIN vt.RTV6 AS v6 ON v6.DocEntry = tk.DocEntry 
                            WHERE tr.DocEntry =  @DocEntry AND (  (r.TipoRuta = 'VG' AND r.TransDesc LIKE '%LIDERMAX%') OR (r.TipoRuta='VD' AND tk.LugarDestino = 'DOMICILIO' AND tk.EntregaPedido IN ('PROVINCIA','RECOJO') ) )
                            AND tr.Estado <> 'LIBERADO'
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
						    v1.TelfPer , r.Placa  , tk.DetallePedido    , r.TipoRuta   , r.Trans2Desc , r.Estado2 , tr.Guias , tr.Peso
                            ORDER BY tk.Agencia ";


            var result = await xCon.QueryAsync<ReporteHojaRutaModel>(sql, new { DocEntry = docEntryHojaRuta });

            return result.ToList();

        }


        public async Task<DireccionProvinciaSuelta_E> ObtenerDireccionProvinciaSuelta(string numAtCard)
        {
            using var xCon = new HanaConnection(dapperContext.hanaConnectionString);

            try
            {
                string query = $@"
            SELECT
                T0.""CardCode"",
                T0.""CardName"",

                T1.""County""  AS ""Departamento"",
                T1.""City""    AS ""Provincia"",
                T1.""Block""   AS ""Distrito"",    
                T0.""Address2"" AS ""DireccionEnvio"",

                T1.""County"" || ' - ' || T1.""City"" || ' - ' || T1.""Block"" || ' - ' || T0.""Address2"" AS ""Direccion1"",

                T0.""SlpCode"",
                T2.""SlpName"" AS ""Vendedor"",
 
                T0.""U_SYP_MDNT"" AS ""NombreTransportista"",
                T0.""U_BPP_NUMBUL"" AS ""NroBultos"",
                T0.""U_CFR_WHS_NET"" AS ""CodigoAlmacen""
            FROM ODLN T0
            LEFT JOIN CRD1 T1 
                ON T0.""CardCode"" = T1.""CardCode""
            LEFT JOIN OSLP T2 
                ON T0.""SlpCode"" = T2.""SlpCode""
            WHERE T0.""NumAtCard"" = :numAtCard";

                var resultado = await xCon.QueryFirstOrDefaultAsync<DireccionProvinciaSuelta_E>(
                    query,
                    new { numAtCard }
                );

                return resultado;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
