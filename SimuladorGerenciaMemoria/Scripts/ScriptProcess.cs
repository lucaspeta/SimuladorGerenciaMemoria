using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SimuladorGerenciaMemoria.Scripts
{
    public class ScriptProcess
    {
        public long MemorySize { get; set; }
        public int MemoryOccupied { get; set; }
        public long FramesSize { get; set; }
        public long FramesQTD { get; set; }
        public Dictionary<long, bool> AllRegBase { get; set; }

        public Memory Memory { get; set; }

        public ScriptProcess(Memory inicialMemory, int memoryO) {
            MemorySize = inicialMemory.Size;
            FramesSize = inicialMemory.FramesSize;
            FramesQTD = inicialMemory.FramesQTD;
            Memory = inicialMemory;

            MemoryOccupied = memoryO;

            AllRegBase = new Dictionary<long, bool>();

            for (long memorySize = 0; memorySize < inicialMemory.Size; memorySize = memorySize + inicialMemory.FramesSize)
                AllRegBase.Add(memorySize, false);
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
        public List<Models.Process> CreateProcesses(string nameProcess = "Process")
        {
            List<int> process = new List<int>(); // process
            List<long> regBase = new List<long>(); // beggining
            List<long> regLimite = new List<long>();  // size
            List<Process> processesToInsert = new List<Process>(); // list to return

            Random p = new Random();

            // Generate the process numbers
            for (int i = 0; process.Count < this.MemoryOccupied; i++)
            {
                int pValue = p.Next(MemoryOccupied);

                if (!process.Contains(pValue))
                    process.Add(pValue);
            }

            for (int i = 0; regLimite.Count < MemoryOccupied; i++)
            {
                long regValor = 0;
                do
                {
                    regValor = p.Next((int)((this.MemorySize) / this.MemoryOccupied));
                } while (regValor.Equals(0));

                if (!regLimite.Contains(regValor))
                {
                    regLimite.Add(regValor);
                    this.MemorySize = this.MemorySize - regLimite[regLimite.Count - 1];
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
                        AllRegBase[regB] = true;
                }
                else regBase.Add(0);
            }

            using (StreamWriter writer = new StreamWriter("Output/Output.txt", false))
            {
                for (int i = 0; i < process.Count; i++)
                {
                    writer.WriteLine(nameProcess + process[i] + ";" + regBase[i] + ";" + regLimite[i]);

                    Models.Process processToAdd = new Models.Process();

                    processToAdd.CreateDate = DateTime.Now;
                    processToAdd.Memory = this.Memory;
                    processToAdd.Name = nameProcess + process[i];                    
                    processToAdd.isInitial = true;
                    processToAdd.RegB = regBase[i];
                    processToAdd.RegL = regLimite[i];

                    processesToInsert.Add(processToAdd);
                }
            }

            return processesToInsert;
        }

        /*
         * long MemoyToFeel -> quantidade da mem�ria que ser� ocupada
         */
        public static List<Models.Process> GerarProcessosList(int memoryID, long MemoryToFeel, int ini, int fin) 
        {
            List<Models.Process> processListToReturn = new List<Models.Process>();
            int processCount = 1;
            long _memoryAvailable = MemoryToFeel;
            Random r = new Random();

            while (_memoryAvailable > 0) 
            {                
                Process processToInsert = new Process();
                long _processSize = r.Next(ini, fin);

                if (_processSize > _memoryAvailable)
                    _processSize = _memoryAvailable;

                processToInsert.Name = "ProcessGenerated_"+processCount;
                processToInsert.MemoryID = memoryID;
                processToInsert.CreateDate = DateTime.Now;
                processToInsert.isInitial = false;

                processToInsert.Size = _processSize;        
                _memoryAvailable -= _processSize;
                processCount++;
                processListToReturn.Add(processToInsert);
            }

            return processListToReturn;
        }
    }
}