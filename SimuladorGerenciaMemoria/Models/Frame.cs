using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        [DisplayName("Nome")]
        public string Name { get; set; }

        [DisplayName("Processo")]
        public int ProcessID { get; set; }
        public virtual Process Process { get; set; }

        [DisplayName("Registrador Base")]
        public long RegB { get; set; }

        [DisplayName("Registrador Limite")]
        public long RegL { get; set; }

        [DisplayName("Memoria")]
        public int MemoryID { get; set; }
        public virtual Memory Memory { get; set; }

        public bool IsInitial { get; set; }

        public enum TipoAlgVal 
        {
            QuickFit,
            FirstFit,
            BestFit,
            WorstFit
        }

        public TipoAlgVal? TipoAlg { get; set; }
    }
}