using System.ComponentModel.DataAnnotations;

namespace SimuladorGerenciaMemoria.Models
{
    public class Login
    {
        [Display(Name = "Email ou Usuário")]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Senha")]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}