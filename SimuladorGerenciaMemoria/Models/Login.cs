using System.ComponentModel.DataAnnotations;

namespace SimuladorGerenciaMemoria.Models
{
    public class Login
    {
        [Display(Name = "Usuário")]
        [Required]
        public string User { get; set; }

        [Display(Name = "Senha")]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}