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
        /*
         * Memory memory -> memoria criada
         * long MemoyToFeel -> quantidade da memória que será ocupada
         * int ini -> tamanho minimo do processo
         * int fin -> tamanho maximo do processo
         */
        public static List<Process> GerarProcessosIniciais(Memory memory, int initialState, int ini, int fin)
        {
            List<Process> processListToReturn = new List<Process>();

            //lista de registradores base disponivel para inserir processos
            List<long> listRegB = new List<long>();
            for (long i = 0; i < memory.Size; i += memory.FramesSize) listRegB.Add(i);

            int processCount = 1;

            double memory_perc = ((double) initialState / 100);

            long _memoryToFeel = (long)(memory.Size * memory_perc);
            
            Random r = new Random();

            while (_memoryToFeel > 0 && listRegB.Count() > 0)
            {
                bool isIndexValid = false;
                Process processToInsert = new Process();
                long _processSize = r.Next(ini, fin);
                long regB = -1;

                long framesNeeded = (_processSize / memory.FramesSize);
                framesNeeded = _processSize % memory.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

                if (_processSize > _memoryToFeel)
                    _processSize = _memoryToFeel;

                //Tenta encontrar um regB para inserir o processo aleatoriamente em 10 tentativas
                for (int tentativas = 0; tentativas < 10; tentativas++)
                {
                    if (listRegB.Count() > (memory.FramesQTD * 0.5))
                        regB = listRegB[r.Next((int)(listRegB.Count()*0.9), (listRegB.Count()-1))];
                    else
                        regB = listRegB[r.Next(0, (listRegB.Count()-1))];                    

                    if (framesNeeded == 1) isIndexValid = true;
                    else 
                    {
                        //verifica se os proximos frames sao validos para insercao
                        for (int j = 0; j < framesNeeded; j++)
                        {
                            long regAtual = regB + (j * memory.FramesSize);
                            
                            if (listRegB.Contains(regAtual))
                            {
                                if (j == (framesNeeded - 1))
                                {
                                    isIndexValid = true;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }                        
                    }

                    if (isIndexValid) break;
                }

                if (isIndexValid) 
                {
                    //remove os registradores que serao usados
                    for (int j = 0; j < framesNeeded; j++)
                        listRegB.Remove(regB + (j * memory.FramesSize));

                    processToInsert.Name = "InitialProcess_" + processCount;
                    processToInsert.Memory = memory;
                    processToInsert.CreateDate = DateTime.Now;
                    processToInsert.isInitial = true;
                    processToInsert.Size = _processSize;
                    processToInsert.RegL = _processSize;
                    processToInsert.RegB = regB;
                    _memoryToFeel -= _processSize;
                    processCount++;
                    processListToReturn.Add(processToInsert);
                }                
            }

            return processListToReturn;
        }

        /*
         * memoryID -> Id da memoria
         * long MemoyToFeel -> quantidade da memória que será ocupada
         * int ini -> tamanho minimo do processo
         * int fin -> tamanho maximo do processo
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