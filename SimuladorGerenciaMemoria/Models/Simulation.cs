using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimuladorGerenciaMemoria.Models
{
    public class Simulation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required(ErrorMessage = "É necessário preencher o nome da simulação.")]
        [DisplayName("Nome")]
        public string Name { get; set; }

        [DisplayName("Usuário")]
        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public IEnumerable<Memory> Memories { get; set; }

        [DisplayName("Data de criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm}")]
        public DateTime CreateDate { get; set; }
    }
}