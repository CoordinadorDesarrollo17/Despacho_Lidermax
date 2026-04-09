using Dapper;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Dtos;
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

        private async Task<IEnumerable<HojasRutaDto>> ObtenerHojasRuta(FiltrosHojasRutaDto model)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"     
                        SELECT TOP 200
                            r.DocEntry,
                            r.DocNum,
                            r.TipoRuta,
                            CONVERT(VARCHAR(10), r.TiempoPac, 103) AS TiempoPac,
                            SUM(tk.Cajas) AS Cajas,
                            r.Estado
                        FROM al.ORRU AS r
                        LEFT JOIN al.RRU0 AS tr ON r.DocEntry = tr.DocEntry 
                        LEFT JOIN vt.ORTV AS tk ON tk.DocEntry = tr.DocEntryTicket 
                        WHERE (tk.EnvioAgencia LIKE '%Domicilio%' OR tk.EnvioAgencia LIKE '%Agencia%')  AND 
                        CONCAT(r.DocNum,r.TipoRuta,CONVERT(VARCHAR(10), r.TiempoPac, 103),r.Estado) LIKE @Buscar
                        GROUP BY r.DocEntry,r.DocNum, r.TipoRuta, r.TiempoPac ,  r.Estado   
                        ORDER BY r.Estado ASC,r.TiempoPac DESC
                    "; //tk.EnvioAgencia IN ('Agencia de transporte','Domicilio del Cliente')

            var result = await xCon.QueryAsync<HojasRutaDto>(sql, new { Buscar = "%" + model.Buscar + "%" });

            return result;
        }

        public async Task<IPagedList<HojasRutaDto>> ListadoHojasRutaPaginados(FiltrosHojasRutaDto model)
        {
            var result = await ObtenerHojasRuta(model);
            return result.ToPagedList(model.Paginacion.Page, model.Paginacion.PageSize);
        }

        public async Task<List<HojasRutaDto>> ListadoHojasRutaExcel(FiltrosHojasRutaDto model)
        {
            var result = await ObtenerHojasRuta(model);
            return result.ToList();
        }
    }
}
