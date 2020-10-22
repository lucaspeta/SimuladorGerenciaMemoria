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
        [DisplayName("Nome")]
        public string Name { get; set; }
        [DisplayName("Data de criação")]
        public DateTime CreateDate { get; set; }
    }
}