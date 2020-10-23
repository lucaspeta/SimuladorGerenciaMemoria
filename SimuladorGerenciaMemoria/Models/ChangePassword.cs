using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SimuladorGerenciaMemoria.Models
{
    public class ChangePassword
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Senha antiga")]
        [Required]
        [DataType(DataType.Password)]
        public string SenhaAntiga { get; set; }

        [Display(Name = "Nova senha")]
        [Required]
        [MinLength(8)]
        [MaxLength(15)]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [Display(Name = "Confirme a nova senha")]
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NovaSenha), ErrorMessage = "As senhas devem ser iguais.")]
        public string RepeteNovaSenha { get; set; }
    }
}