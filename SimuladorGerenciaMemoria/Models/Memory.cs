using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SimuladorGerenciaMemoria.Classes;
using SimuladorGerenciaMemoria.Utils;

namespace SimuladorGerenciaMemoria.Models
{
    public class Memory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [DisplayName("Nome da memória")]

        [Required(ErrorMessage = "É necessário preencher o nome da memória.")]
        public string Name { get; set; }

        public string InitialStateVal { get; set; }

        public enum InitialStatePickList
        {
            [Display(Name = "25%")]
            Pequeno,
            [Display(Name = "50%")]
            Medio,
            [Display(Name = "75%")]
            Grande
        }

        [DisplayName("Estado inicial")]
        public InitialStatePickList InitialState { get; set; }

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
        public DateTime CreateDate { get; set; }

        public IEnumerable<Frame> Frames { get; set; }

        public IEnumerable<Process> Processes { get; set; }

        [DisplayName("Gerada lista processos?")]
        public bool IsGeneratedProcessList { get; set; }
    }

    /*private void InitializeMemory(List<Process> Processes)
    {
        Dictionary<long, Process> mapProcess = new Dictionary<long, Process>();

        foreach (var forProcess in Processes)
        {
            mapProcess.Add(forProcess.RegB, forProcess);
        }

        int i = 0;

        while (i < FramesQTD)
        {
            long memoryframeLocation = (i * Utils.IntPow(2, 10));

            if (mapProcess.ContainsKey(memoryframeLocation))
            {
                Process processToInsertMemory = mapProcess[memoryframeLocation];

                if (processToInsertMemory.RegL > this.FramesSize)
                {
                    decimal framesNeeded = Math.Ceiling((decimal)processToInsertMemory.RegL / this.FramesSize);

                    for (int i2 = 0; i2 < framesNeeded; i2++)
                    {
                        memoryframeLocation = (i * Utils.IntPow(2, 10));

                        Frame frameToInsert = new Frame(memoryframeLocation);
                        frameToInsert.Process = processToInsertMemory;

                        this.Frames.Add(frameToInsert);

                        i++;
                    }
                }
                else
                {
                    Frame frameToInsert = new Frame(memoryframeLocation);
                    frameToInsert.Process = processToInsertMemory;

                    this.Frames.Add(frameToInsert);
                    i++;
                }
            }
            else
            {
                Frame frameToInsert = new Frame(memoryframeLocation);
                frameToInsert.Process = null;

                this.Frames.Add(frameToInsert);
                i++;
            }
        }
    }*/
}