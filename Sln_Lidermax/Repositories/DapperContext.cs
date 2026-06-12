namespace Sln_Lidermax.Repositories
{
    public class DapperContext
    {
        public readonly string connectionString;
        public readonly string hanaConnectionString;
        public DapperContext(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
            hanaConnectionString = config.GetConnectionString("HanaConnection");
        }
    }
}
