using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimuladorGerenciaMemoria.Models
{
    public class Frame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }
        public Process Process { get; set; }

        public long RegB { get; set; }

        /*public Frame(long )
        {
            RegB = rb;
        }*/
    }
}