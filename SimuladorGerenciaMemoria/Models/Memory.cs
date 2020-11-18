using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SimuladorGerenciaMemoria.Utils;

namespace SimuladorGerenciaMemoria.Models
{
    public class Memory {     
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [DisplayName("Nome da memória")]

        [Required(ErrorMessage = "É necessário preencher o nome da memória.")]
        public string Name { get; set; }

        public string InitialStateVal { get; set; }        

        [DisplayName("Estado inicial")]
        public int InitialState { get; set; }

        [DisplayName("Tamanho mínimo dos processos iniciais (bytes)")]
        public int InitialProcessMin { get; set; }

        [DisplayName("Tamanho máximo dos processos iniciais (bytes)")]
        public int InitialProcessMax { get; set; }

        [DisplayName("Tamanho")]
        public long Size { get; set; }
        [DisplayName("FrameTamanho")]
        public long FramesSize { get; set; }
        [DisplayName("FrameQTD")]
        public long FramesQTD { get; set; }

        [Display(Name = "Simulação")]
        public int SimulationID { get; set; }

        [ForeignKey("SimulationID")]
        public virtual Simulation Simulation { get; set; }

        [DisplayName("Dt Criação")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreateDate { get; set; }

        public IEnumerable<Frame> Frames { get; set; }

        public IEnumerable<Process> Processes { get; set; }

        [DisplayName("Usuário")]
        public int? UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [DisplayName("Gerada lista processos?")]
        public bool IsGeneratedProcessList { get; set; }
        public bool IsFirstFitCompleted { get; set; }
        public bool IsNextFitCompleted { get; set; }
        public bool IsBestFitCompleted { get; set; }
        public bool IsWorstFitCompleted { get; set; }

        public int FirstFitInseridos { get; set; }
        public int NextFitInseridos { get; set; }
        public int BestFitInseridos { get; set; }
        public int WorstFitInseridos { get; set; }
    }    
}