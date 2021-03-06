﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace SimuladorGerenciaMemoria.Models
{
    [Table("Users")]
    public partial class User
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(40)]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Usuário")]
        public string Login { get; set; }

        [Required]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Comfirme a senha")]
        [NotMapped]
        [Compare(nameof(Password), ErrorMessage = "As senhas devem ser iguais.")]
        public string RepeatPassword { get; set; }

        public IEnumerable<Simulation> Simulations { get; set; }

        public IEnumerable<Memory> Memories { get; set; }
    }
}
