using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimuladorGerenciaMemoria.Models
{
    public class Process
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [DisplayName("Nome")]
        public string Name { get; set; }

        [DisplayName("Tamanho")]
        public int Size { get; set; }

        [DisplayName("Data de criação")]
        public DateTime CreateDate { get; set; }

        public int? MemoryID { get; set; }

        public virtual Memory Memory { get; set; }

        public IEnumerable<Frame> Frames { get; set; }
    }
}