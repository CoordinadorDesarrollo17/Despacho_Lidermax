namespace Sln_Lidermax.Repositories
{
    public class DapperContext
    {
        public readonly string connectionString;

        public DapperContext(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }
    }
}
