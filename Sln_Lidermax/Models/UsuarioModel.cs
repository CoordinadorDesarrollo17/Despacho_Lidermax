namespace Sln_Lidermax.Models
{
    public class UsuarioModel
    {
        public int IdUsuario { get; set; }  
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public int IdRol { get; set; }
        public int? Activo { get; set; }
        public string? Prefijo { get; set; }
        public string? Email { get; set; }  
        public string UsuarioConcatenado { get; set; }
        public string? PasswordHash { get; set; }
    }
}
