using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimuladorGerenciaMemoria.Classes
{
    public class ScriptProcess
    {
        public long memorySize { get; set; }
        public int memoryOccupied { get; set; }
        public long FramesSize { get; set; }
        public long FramesQTD { get; set; }


        public Dictionary<long, bool> AllRegBase { get; set; }

        public ScriptProcess(Memory inicialMemomry, int memoryO) {
            memorySize = inicialMemomry.Size;
            FramesSize = inicialMemomry.FramesSize;
            FramesQTD = inicialMemomry.FramesQTD;
            memoryOccupied = memoryO;
            AllRegBase = new Dictionary<long, bool>();
            for (long memorySize = 0; memorySize < inicialMemomry.Size; memorySize = memorySize + inicialMemomry.FramesSize)
            {
                AllRegBase.Add(memorySize, false);
            }
        }
        


    public List<long> GetRegBase(long framesNeeded)
        {
            List<long> validIndexes = new List<long>();
            if (AllRegBase != null)
            {
                var regBase = new Dictionary<long, bool>();
                int regBaseValid = 0;
                long firstIndexValid = -1;
                foreach (var item in AllRegBase)
                {
                    if (!item.Value)
                    {
                        if (firstIndexValid.Equals(-1))
                            firstIndexValid = item.Key;
                        regBaseValid++;
                    }
                    else
                    {
                        if (regBaseValid >= framesNeeded && !firstIndexValid.Equals(-1))
                        {
                            for (long i = firstIndexValid; i <= firstIndexValid + (this.FramesSize * (regBaseValid - framesNeeded)); i += this.FramesSize)
                            {
                                var currentRegB = AllRegBase.Where(w => w.Key.Equals(i)).FirstOrDefault();
                                regBase.Add(currentRegB.Key, currentRegB.Value);
                                validIndexes.Add(currentRegB.Key);
                            }
                            firstIndexValid = -1;
                        }
                        regBaseValid = 0;
                    }
                }
                if (!validIndexes.Any(w => w.Equals(firstIndexValid)))
                {
                    if (regBaseValid >= framesNeeded)
                    {
                        for (long i = firstIndexValid; i <= firstIndexValid + (this.FramesSize * (regBaseValid - framesNeeded)); i += this.FramesSize)
                        {
                            var currentRegB = AllRegBase.Where(w => w.Key.Equals(i)).FirstOrDefault();
                            regBase.Add(currentRegB.Key, currentRegB.Value);
                            validIndexes.Add(currentRegB.Key);
                        }
                    }
                }
            }
            else
                validIndexes.Add(0);

            return validIndexes;
        }

        
        public void CreateFile( string nameProcess = "Process")
        {

            List<int> process = new List<int>(); // process
            List<long> regBase = new List<long>(); // beggining
            List<long> regLimite = new List<long>();  // size

            Random p = new Random();

            // Generate the process numbers
            for (int i = 0; process.Count < this.memoryOccupied; i++)
            {
                int pValue = p.Next(memoryOccupied);
              
                if (!process.Contains(pValue))
                    process.Add(pValue);
            }

            for (int i = 0; regLimite.Count < memoryOccupied; i++)
            {
                long regValor = 0;
                do
                {
                    regValor = p.Next((int)((this.memorySize) / this.memoryOccupied));

                } while (regValor.Equals(0));

                if (!regLimite.Contains(regValor))
                {
                    regLimite.Add(regValor);
                    this.memorySize = this.memorySize - regLimite[regLimite.Count - 1];
                }
            }

            process.Sort();

            for (int i = 0; i < regLimite.Count; i++)
            {
               
                long framesNeeded = 0;
                framesNeeded += (regLimite[i] / this.FramesSize);
                framesNeeded = regLimite[i] % this.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

                var avaiableRegB = GetRegBase(framesNeeded);

                if (!avaiableRegB.Count.Equals(0) && AllRegBase != null)
                {
                    
                    Random randomRegB = new Random();
                    long regB = randomRegB.Next(0, avaiableRegB.Count);
                    regB = avaiableRegB[Convert.ToInt32(regB)];
                    regBase.Add(regB);
                    for (long usedFrames = 1; usedFrames <= framesNeeded; usedFrames++, regB += FramesSize)
                    {
                        AllRegBase[regB] = true;
                    }
                }
                else
                    regBase.Add(0);
            }

            using (StreamWriter writer = new StreamWriter("Output/Output.txt", false))
            {
                for (int i = 0; i < process.Count; i++)
                    writer.WriteLine(nameProcess + process[i] + ";" + regBase[i] + ";" + regLimite[i]);
            }
        }
    }
}