using Dapper;
using Microsoft.Data.SqlClient;
using Sln_Lidermax.Interfaces;
using Sln_Lidermax.Models;

namespace Sln_Lidermax.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DapperContext dapperContext;

        public AuthRepository(DapperContext dapperContext)
        {
            this.dapperContext = dapperContext;
        }

        public async Task<UsuarioModel?> BuscarUsuario(string usuario)
        {
            using var xCon = new SqlConnection(dapperContext.connectionString);

            var sql = @"
                    SELECT 
                        DocEntry AS IdUsuario,
                        Nombres,
                        Apellidos,
                        Email,
                        IdRol,
                        Prefijo,
                        Activo,
                        (TRIM(Prefijo) + TRIM(Id)) AS UsuarioConcatenado,
                        CONVERT(VARCHAR(MAX), DECRYPTBYPASSPHRASE(@Passphrase, [Password])) AS PasswordHash
                    FROM [dbo].[OUSR]
                    WHERE (TRIM(Prefijo) + TRIM(Id)) = @Usuario AND IdRol IN (1,72,73)";

            var parametros = new
            {
                Usuario = usuario,
                Passphrase = "pwC0B3F@R"
            };

            return await xCon.QuerySingleOrDefaultAsync<UsuarioModel>(sql, parametros);
        }


    }
}
