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
        public long Size { get; set; }

        [DisplayName("Data de criação")]
        public DateTime CreateDate { get; set; }

        public int? MemoryID { get; set; }

        public virtual Memory Memory { get; set; }

        public IEnumerable<Frame> Frames { get; set; }

        [DisplayName("Registrador base")]
        public long RegB { get; set; }

        [DisplayName("Registrador limite")]
        public long RegL { get; set; }
        public bool isInitial { get; set; }

        public long TimeToFindIndex { get; set; }
    }
}