using System.ComponentModel.DataAnnotations;

namespace Sln_Lidermax.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Usuario")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
    }
}
