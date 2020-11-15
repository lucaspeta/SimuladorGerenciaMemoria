using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimuladorGerenciaMemoria.Models
{
    //InnerClass para gerenciamento de espaços livres
    public class EspacoLivre 
    {
        public long RegB { get; set; }
        public int EspacosLivres { get; set; }
    }

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

        [DisplayName("Memoria")]
        public int MemoryID { get; set; }
        public virtual Memory Memory { get; set; }

        public bool IsInitial { get; set; }

        public enum TipoAlgVal 
        {            
            FirstFit,
            NextFit,
            BestFit,
            WorstFit
        }

        public TipoAlgVal? TipoAlg { get; set; }

        public int FrameNumber { get; set; }

        public int FrameSize { get; set; }

        public int CapacidadeUtilizada { get; set; }
    }
}