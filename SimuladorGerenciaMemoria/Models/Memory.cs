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
        //public static long MemorySize = 2500 * Utils.IntPow(2, 10);
        //public static long FrameSize = 1 * Utils.IntPow(2, 10);

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [DisplayName("Nome da memória")]

        [Required(ErrorMessage = "É necessário preencher o nome da memória.")]
        public string Name { get; set; }

        public enum SizePickList 
        {
            Pequeno,
            Medio,
            Grande
        }

        [DisplayName("Tamanho")]
        public long Size { get; set; }
        [DisplayName("Tamanho dos frames")]
        public long FramesSize { get; set; }
        [DisplayName("Quantidade de frames")]
        public long FramesQTD { get; set; }

        [Display(Name = "Simulação")]
        public int SimulationID { get; set; }

        [ForeignKey("SimulationID")]
        public virtual Simulation Simulation { get; set; }

        [DisplayName("Data de criação")]
        public DateTime CreateDate { get; set; }

        public IEnumerable<Frame> Frames { get; set; }

        public IEnumerable<Process> Processes { get; set; }

        public bool isGeneratedProcessList { get; set; }

        public enum InitialState 
        {

        }
    }

    /*private Memory GetOriginalMemory
    {
        get
        {
            Memory originalMemory = new Memory(MemorySize, FrameSize);

            String Buffer = Utils.ReadInputFile("../../../Inputs/data.csv");
            List<Process> processesList = Utils.CsvToProcessList(Buffer);
            originalMemory.InitializeMemory(processesList);

            return originalMemory;
        }
    }*/

    /*public Memory()
    {
        //Frames = GetOriginalMemory.Frames;
        Size = GetOriginalMemory.Size;
        FramesSize = GetOriginalMemory.FramesSize;
        FramesQTD = GetOriginalMemory.FramesQTD;
    }

    public Memory(long ms, long fs)
    {
        //Frames = new List<Frame>();
        Size = ms;
        FramesSize = fs;
        FramesQTD = ms / fs;
    }*/

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