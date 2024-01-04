
using System.ComponentModel.DataAnnotations;

namespace LotteryResult.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Nombre de usuario requerido.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Contraseña requerida.")]
        public string Password { get; set; }
    }
}
