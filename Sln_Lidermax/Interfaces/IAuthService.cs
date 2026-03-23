namespace Sln_Lidermax.Interfaces
{
    public interface IAuthService
    {
        bool ValidateUser(string username, string password);
    }
}
