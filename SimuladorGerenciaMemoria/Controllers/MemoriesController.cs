using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;
using SimuladorGerenciaMemoria.Scripts;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace SimuladorGerenciaMemoria.Controllers
{
    public class MemoriesController : Controller
    {
        private readonly SimuladorContext _context;

        public MemoriesController(SimuladorContext context)
        {
            _context = context;
        }

        // GET: Memories
        [RedirectAction]
        public async Task<IActionResult> Index()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            return View(
                await _context.Memories
                .Include(m => m.Simulation)
                .Where(m => m.UserID == HttpContext.Session.GetInt32("UserID"))
                .OrderBy(m => m.CreateDate)
                .ToListAsync()
                );
        }

        // GET: Memories/Details/5
        [RedirectAction]
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.memoriaID = id;

            ViewBag.processQTD = _context.Processes
                .Where(p => p.isInitial == false)
                .Where(p => p.MemoryID == id)
                .Count();

            if (id == null)
                return RedirectToAction("Error404", "Erros");

            var memory = await _context.Memories
                .Include(m => m.Simulation)
                .Include(m => m.Processes)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (memory == null)
                return RedirectToAction("Error404", "Erros");

            if (memory.UserID != HttpContext.Session.GetInt32("UserID"))
                return RedirectToAction("Error403", "Erros");

            if (memory.IsFirstFitCompleted)
                ViewBag.First = memory.IsFirstFitCompleted;


            if (memory.IsNextFitCompleted)
                ViewBag.Next = memory.IsNextFitCompleted;

            if (memory.IsBestFitCompleted)
                ViewBag.Best = memory.IsBestFitCompleted;


            if (memory.IsWorstFitCompleted)
                ViewBag.Worst = memory.IsWorstFitCompleted;

            var framesInicial = await _context.Frames
                .Where(f => f.MemoryID == id && f.IsInitial == true) 
                .ToListAsync();

            //charts data

            //Map: int -> FrameNumber, int -> FrameID 
            Dictionary<int, int> mapFrameUsed = new Dictionary<int, int>();

            long memoriaUsada = 0;            
            long memoriaInutil = 0;

            List<int> framesLivres = new List<int>{0,0,0,0,0,0,0,0,0,0};                       

            foreach (var item in framesInicial) {
                mapFrameUsed.Add(item.FrameNumber, item.ID);

                if (item.CapacidadeUtilizada != memory.FramesSize) 
                    memoriaInutil += (memory.FramesSize - item.CapacidadeUtilizada);

                memoriaUsada += memory.FramesSize;
            }

            for (int i = 0; i < memory.FramesQTD; i++) 
            {
                //descobre a qual porcentagem da memoria o frame se encontra
                double porc = ((i * 100) / memory.FramesQTD);

                if (porc <= 10) if (!mapFrameUsed.ContainsKey(i)) framesLivres[0] += 1;
                if (porc <= 20 && porc > 10) if (!mapFrameUsed.ContainsKey(i)) framesLivres[1] += 1;
                if (porc <= 30 && porc > 20) if (!mapFrameUsed.ContainsKey(i)) framesLivres[2] += 1;
                if (porc <= 40 && porc > 30) if (!mapFrameUsed.ContainsKey(i)) framesLivres[3] += 1;
                if (porc <= 50 && porc > 40) if (!mapFrameUsed.ContainsKey(i)) framesLivres[4] += 1;
                if (porc <= 60 && porc > 50) if (!mapFrameUsed.ContainsKey(i)) framesLivres[5] += 1;
                if (porc <= 70 && porc > 60) if (!mapFrameUsed.ContainsKey(i)) framesLivres[6] += 1;
                if (porc <= 80 && porc > 70) if (!mapFrameUsed.ContainsKey(i)) framesLivres[7] += 1;
                if (porc <= 90 && porc > 80) if (!mapFrameUsed.ContainsKey(i)) framesLivres[8] += 1;
                if (porc <= 100 && porc > 90) if (!mapFrameUsed.ContainsKey(i)) framesLivres[9] += 1;
            }

            long memoriaLivre = memory.Size - memoriaUsada;

            ViewBag.memoriaUsada = ((double) memoriaUsada / 1024); //KiB
            ViewBag.memoriaLivre = ((double) memoriaLivre / 1024); //KiB
            ViewBag.memoriaInutil = ((double) memoriaInutil / 1024); //KiB
            ViewBag.framesLivres = JsonConvert.SerializeObject(framesLivres);

            return View(memory);
        }

        // GET: Memories/Create
        [RedirectAction]
        public IActionResult Create()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.SimulationID = new SelectList(_context.Simulations.OrderByDescending(s => s.CreateDate), "ID", "Name");
            ViewBag.UserID = HttpContext.Session.GetInt32("UserID");

            return View();
        }

        [HttpPost]
        [RedirectAction]
        public JsonResult Create(string Name, int SimulationID, long Size, long FramesSize, int InitialState, int InitialProcessMin, int InitialProcessMax)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.SimulationID = new SelectList(_context.Simulations.Where(s => s.UserID == HttpContext.Session.GetInt32("UserID")), "ID", "Name");
            ViewBag.UserID = HttpContext.Session.GetInt32("UserID");

            try
            {
                if (Name == null || SimulationID == 0 || Size == 0 || FramesSize == 0 || InitialState == 0 || InitialProcessMin == 0 || InitialProcessMax == 0)
                    throw new Exception("É necessário preencher os campos obrigatórios.");

                Memory memory = new Memory
                {
                    Name = Name,
                    UserID = HttpContext.Session.GetInt32("UserID"),
                    IsGeneratedProcessList = false,
                    SimulationID = SimulationID,
                    Size = Size,
                    FramesSize = FramesSize,
                    InitialState = InitialState,
                    InitialProcessMin = InitialProcessMin,
                    InitialProcessMax = InitialProcessMax
                };

                if (memory.InitialProcessMin > memory.InitialProcessMax) 
                    throw new Exception("O tamanho máximo do processo precisa ser maior que o mínimo.");                

                memory.Size = Size * 1024; //transfoma em bytes
                memory.CreateDate = DateTime.Now;
                memory.FramesQTD = memory.Size / memory.FramesSize;
                memory.IsFirstFitCompleted = false;
                memory.IsNextFitCompleted = false;
                memory.IsBestFitCompleted = false;
                memory.IsWorstFitCompleted = false;

                int initialState = memory.InitialState;               

                _context.Add(memory);

                int processesNeeded = (int)memory.FramesQTD * initialState / 100;

                List<Models.Process> processList = ScriptProcess.GerarProcessosIniciais(memory, initialState, memory.InitialProcessMin, memory.InitialProcessMax);
                List<Models.Frame> framesList = new List<Models.Frame>();

                //generate the frames
                foreach (var item in processList)
                {
                    var framesNeeded = item.RegL / memory.FramesSize;
                    framesNeeded = item.RegL % memory.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

                    for (int i = 0; i < framesNeeded; ++i)
                    {
                        Frame frameToAdd = new Frame();

                        frameToAdd.Memory = memory;
                        frameToAdd.Name = item.Name;
                        frameToAdd.IsInitial = true;
                        frameToAdd.Process = item;
                        frameToAdd.RegB = item.RegB + (i * memory.FramesSize);
                        frameToAdd.FrameNumber = frameToAdd.RegB > 0 ? (int)(frameToAdd.RegB / memory.FramesSize) : 0;
                        frameToAdd.FrameSize = (int)memory.FramesSize;

                        //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                        if (i + 1 == framesNeeded)
                            frameToAdd.CapacidadeUtilizada = (int)(item.RegL % memory.FramesSize);
                        else
                            frameToAdd.CapacidadeUtilizada = (int)memory.FramesSize;

                        framesList.Add(frameToAdd);
                    }
                }

                _context.AddRange(processList);
                _context.AddRange(framesList);

                _context.SaveChangesAsync();

                return Json(
                   new
                   {
                       success = true                       
                   }
               );
            }
            catch (Exception e) 
            {
                return Json(
                   new
                   {
                       success = false,
                       error = e.Message
                   }
               );
            }            
        }

        // GET: Memories/Edit/5
        [RedirectAction]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.MemoryID = id;

            if (id == null) return RedirectToAction("Error404", "Erros");

            var memory = await _context.Memories.FindAsync(id);

            if (memory == null) return RedirectToAction("Error404", "Erros");

            if (memory.UserID != HttpContext.Session.GetInt32("UserID"))
                return RedirectToAction("Error403", "Erros");

            return View(memory);
        }

        // POST: Memories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,CreateDate,Size,FramesSize,FramesQTD,SimulationID,IsGeneratedProcessList,UserID,IsBestFitCompleted,IsFirstFitCompleted,IsWorstFitCompleted,IsNextFitCompleted,InitialState,InitialProcessMin,InitialProcessMax,BestFitInseridos,FirstFitInseridos,WorstFitInseridos,NextFitInseridos")] Memory memory)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id != memory.ID)
            {
                return RedirectToAction("Error404", "Erros");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(memory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemoryExists(memory.ID))
                        return RedirectToAction("Error404", "Erros");
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(memory);
        }

        [RedirectAction]
        public async Task<IActionResult> GerarProcessos(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.MemoryID = id;

            Memory _memory = _context.Memories.Find(id);

            ViewBag.isGenerated = _memory.IsGeneratedProcessList;

            if (_memory.UserID != HttpContext.Session.GetInt32("UserID"))
                return RedirectToAction("Error403", "Erros");
    
            ViewBag.MaximoPossivel = 120 - _memory.InitialState;
    
            if (id == null) return RedirectToAction("Error404", "Erros");

            return View(
               await _context.Processes
               .Where(p => p.MemoryID == id)
               .Where(p => p.isInitial == false)
               .ToListAsync()
               );
        }

        [RedirectAction]
        [HttpPost]
        public JsonResult GerarProcessosInserir(int memoryID, long MemoryToFeelPerc, int ini, int fin)
        {
            try
            {
                if(ini > fin)
                    throw new Exception("O tamanho máximo do processo precisa ser maior que o mínimo!");

                Memory _memory = _context.Memories.Find(memoryID);

                if (_memory == null)
                    throw new Exception("Houve um erro! Memória não encontrada.");

                if ((_memory.InitialState+MemoryToFeelPerc) > 120)
                    throw new Exception("Valor para preencher inválido!");

                long _memoryToFeel = (long)(_memory.Size * ((float)MemoryToFeelPerc / 100));

                List<Models.Process> processToInsert = ScriptProcess.GerarProcessosList(memoryID, _memoryToFeel, ini, fin);

                _memory.IsGeneratedProcessList = true;
                _context.Processes.AddRange(processToInsert);
                _context.SaveChangesAsync();

                if (_memory.UserID != HttpContext.Session.GetInt32("UserID"))
                    throw new Exception("Você não possui permissão para gerar processos para essa memória!");

                return Json(
                    new
                    {
                        success = true
                    }
                );
            }
            catch (Exception e)
            {
                return Json(
                   new
                   {
                       success = false,
                       error = e.Message
                   }
               );
            }
        }

        [RedirectAction]
        private bool MemoryExists(int id)
        {
            return _context.Memories.Any(e => e.ID == id);
        }

        //Processos que foram gerados para a simulação
        private List<Models.Process> GetProcessesToInsert(int? id)
        {
            List<Models.Process> processToInsert = _context.Processes
                .Where(p => p.MemoryID == id)
                .Where(p => p.isInitial == false)
                .ToList();

            return processToInsert;
        }

        //Frames Inicialmente ocupados
        private List<Models.Frame> GetInitialFrames(int? id)
        {
            List<Models.Frame> framesIniciais = _context.Frames
                .Where(f => f.MemoryID == id)
                .Where(f => f.IsInitial == true)
                .OrderBy(f => f.RegB)
                .ToList();

            return framesIniciais;
        }

        //Gera a lista para gerenciar espaços livres
        private List<EspacoLivre> GeraListaLivres(Memory memory, List<Frame> InitialFrames)
        {
            //lista de espacos livres na memoria
            List<EspacoLivre> listaEspacoLivre = new List<EspacoLivre>();

            //long -> RegB, int -> id(Frame), 
            Dictionary<long, int> mapFrame = new Dictionary<long, int>();

            //Mapeia todos os frames utilizados
            foreach (var item in InitialFrames)
                mapFrame.Add(item.RegB, item.ID);

            //frames disponiveis
            int framesLivres = 0;

            //guarda o regB do primeiro frame livre
            long regB = 0;

            //passa por todos registradores base da memória
            for (long i = 0; i < memory.Size; i += memory.FramesSize)
            {
                //verifica se o frame está livre
                //(está livre)
                if (!mapFrame.ContainsKey(i))
                {
                    //guarda o registrador base do primerio frame disponivel
                    if (framesLivres == 0) regB = i;

                    //soma a quantidade de frames livres
                    framesLivres++;
                }
                //(Não está livre)
                else
                {
                    //Verifica se é necessário criar o objeto
                    if (framesLivres != 0)
                    {
                        EspacoLivre espacoLivre = new EspacoLivre
                        {
                            EspacosLivres = framesLivres,
                            RegB = regB
                        };

                        listaEspacoLivre.Add(espacoLivre);

                        //reseta a quantidade de frames livres
                        framesLivres = 0;
                    }
                }
            }

            return listaEspacoLivre;
        }

        [HttpPost]
        public JsonResult AlgSimulation(int? id, string Alg)
        {
            try
            {
                if (id != null)
                {
                    Memory memory = _context.Memories.Find(id);

                    if (!memory.IsGeneratedProcessList) 
                        throw new Exception("É necessário gerar a lista de processos.");

                    string algSimuExistenteError = "Já foi gerada a simulação para esse algoritmo.";

                    if(memory.IsFirstFitCompleted && Alg == "FirstFit")
                        throw new Exception(algSimuExistenteError);

                    if (memory.IsNextFitCompleted && Alg == "NextFit")
                        throw new Exception(algSimuExistenteError);

                    if (memory.IsBestFitCompleted && Alg == "BestFit")
                        throw new Exception(algSimuExistenteError);

                    if (memory.IsWorstFitCompleted && Alg == "WorstFit")
                        throw new Exception(algSimuExistenteError);

                    List<Process> processToInsert = GetProcessesToInsert(id);
                    List<Frame> initialFrames = GetInitialFrames(id);
                    List<Frame> framesToInsert = new List<Frame>();
                    List<EspacoLivre> espacosLivres = GeraListaLivres(memory, initialFrames);

                    int processInserted = 0;

                    // tempo de execução para acha o index do processo
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    foreach (var process in processToInsert)
                    {
                        watch.Start();
                        int framesNeeded = (int)(process.Size / memory.FramesSize);
                        framesNeeded = process.Size % memory.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

                        if (Alg == "FirstFit") 
                        {
                            for (int i = 0; i < espacosLivres.Count; ++i)
                            {
                                //se o processo caber no espaco disponivel na memória
                                if (espacosLivres[i].EspacosLivres >= framesNeeded)
                                {
                                    for (int j = 0; j < framesNeeded; ++j)
                                    {
                                        Frame newFrame = new Frame
                                        {
                                            IsInitial = false,
                                            RegB = espacosLivres[i].RegB + (memory.FramesSize * j),
                                            TipoAlg = Frame.TipoAlgVal.FirstFit,
                                            MemoryID = memory.ID,
                                            ProcessID = process.ID,
                                            Name = process.Name,
                                            FrameSize = (int)memory.FramesSize
                                        };

                                        newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(newFrame.RegB / memory.FramesSize) : 0;

                                        //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                        if (j + 1 == framesNeeded)
                                            newFrame.CapacidadeUtilizada = (int)(process.Size % memory.FramesSize);
                                        else
                                            newFrame.CapacidadeUtilizada = (int)memory.FramesSize;

                                        framesToInsert.Add(newFrame);
                                    }

                                    //se for necessario remove o item da lista de livre
                                    if (espacosLivres[i].EspacosLivres - framesNeeded == 0)
                                        espacosLivres.RemoveAt(i);
                                    else
                                    {
                                        //Atualiza a lista de espacos livres
                                        int quantidadeLivreAnt = espacosLivres[i].EspacosLivres;
                                        long regBAnt = espacosLivres[i].RegB;

                                        espacosLivres[i] = new EspacoLivre
                                        {                                            
                                            RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                            EspacosLivres = quantidadeLivreAnt - framesNeeded
                                        };
                                    }

                                    watch.Stop();
                                    // Get the elapsed time as a TimeSpan value.
                                    TimeSpan ts = watch.Elapsed;
                                    process.TimeToFindIndexFirst = ts.Milliseconds;

                                    processInserted++;                                   
                                    break;
                                }
                            }
                        }

                        if (Alg == "NextFit") 
                        {
                            bool isFirst = true;

                            for (int i = 0; i < espacosLivres.Count; ++i)
                            {
                                //se o processo caber no espaco disponivel na memória
                                if (espacosLivres[i].EspacosLivres >= framesNeeded && !isFirst)
                                {
                                    for (int j = 0; j < framesNeeded; ++j)
                                    {
                                        Frame newFrame = new Frame
                                        {
                                            IsInitial = false,
                                            RegB = espacosLivres[i].RegB + (memory.FramesSize * j),
                                            TipoAlg = Frame.TipoAlgVal.NextFit,
                                            MemoryID = memory.ID,
                                            ProcessID = process.ID,
                                            Name = process.Name,
                                            FrameSize = (int)memory.FramesSize
                                        };

                                        newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(newFrame.RegB / memory.FramesSize) : 0;

                                        //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                        if (j + 1 == framesNeeded)
                                            newFrame.CapacidadeUtilizada = (int)(process.Size % memory.FramesSize);
                                        else
                                            newFrame.CapacidadeUtilizada = (int)memory.FramesSize;

                                        framesToInsert.Add(newFrame);
                                    }

                                    //se for necessario remove o item da lista de livre
                                    if (espacosLivres[i].EspacosLivres - framesNeeded == 0)
                                        espacosLivres.RemoveAt(i);
                                    else
                                    {
                                        //Atualiza a lista de espacos livres
                                        int quantidadeLivreAnt = espacosLivres[i].EspacosLivres;
                                        long regBAnt = espacosLivres[i].RegB;

                                        espacosLivres[i] = new EspacoLivre
                                        {                                            
                                            RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                            EspacosLivres = quantidadeLivreAnt - framesNeeded
                                        };
                                    }

                                    processInserted++;

                                    watch.Stop();
                                    // Get the elapsed time as a TimeSpan value.
                                    TimeSpan ts = watch.Elapsed;
                                    process.TimeToFindIndexNext = ts.Milliseconds;
                                    break;
                                }

                                if (espacosLivres[i].EspacosLivres >= framesNeeded)
                                    if (isFirst) isFirst = false;
                            }                            
                        }

                        if (Alg == "BestFit") 
                        {
                            int? melhorEspaco = null;

                            for (int i = 0; i < espacosLivres.Count; ++i)
                            {
                                if (espacosLivres[i].EspacosLivres >= framesNeeded) 
                                {
                                    if (melhorEspaco == null)
                                        melhorEspaco = i;
                                    else 
                                    {
                                        if (espacosLivres[(int)melhorEspaco].EspacosLivres > espacosLivres[i].EspacosLivres) 
                                            melhorEspaco = i;                                 
                                    }                                   
                                }
                            }

                            //se o processo caber no espaco disponivel na memória
                            if (melhorEspaco != null)
                            {
                                for (int j = 0; j < framesNeeded; ++j)
                                {
                                    Frame newFrame = new Frame
                                    {
                                        IsInitial = false,
                                        RegB = espacosLivres[(int)melhorEspaco].RegB + (memory.FramesSize * j),
                                        TipoAlg = Frame.TipoAlgVal.BestFit,
                                        MemoryID = memory.ID,
                                        ProcessID = process.ID,
                                        Name = process.Name,
                                        FrameSize = (int)memory.FramesSize
                                    };

                                    newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(newFrame.RegB / memory.FramesSize) : 0;

                                    //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                    if (j + 1 == framesNeeded)
                                        newFrame.CapacidadeUtilizada = (int)(process.Size % memory.FramesSize);
                                    else
                                        newFrame.CapacidadeUtilizada = (int)memory.FramesSize;

                                    framesToInsert.Add(newFrame);
                                }

                                //se for necessario remove o item da lista de livre
                                if (espacosLivres[(int)melhorEspaco].EspacosLivres - framesNeeded == 0)
                                    espacosLivres.RemoveAt((int)melhorEspaco);
                                else
                                {
                                    //Atualiza a lista de espacos livres
                                    int quantidadeLivreAnt = espacosLivres[(int)melhorEspaco].EspacosLivres;
                                    long regBAnt = espacosLivres[(int)melhorEspaco].RegB;

                                    espacosLivres[(int)melhorEspaco] = new EspacoLivre
                                    {                                        
                                        RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                        EspacosLivres = quantidadeLivreAnt - framesNeeded
                                    };
                                }

                                processInserted++;

                                watch.Stop();
                                // Get the elapsed time as a TimeSpan value.
                                TimeSpan ts = watch.Elapsed;

                                process.TimeToFindIndexBest = ts.Milliseconds;
                            }
                        }

                        if (Alg == "WorstFit")
                        {
                            int? piorEspaco = null;

                            for (int i = 0; i < espacosLivres.Count; ++i)
                            {
                                if (espacosLivres[i].EspacosLivres >= framesNeeded)
                                {
                                    if (piorEspaco == null)
                                        piorEspaco = i;
                                    else
                                    {
                                        if (espacosLivres[(int)piorEspaco].EspacosLivres < espacosLivres[i].EspacosLivres)
                                        {
                                            piorEspaco = i;
                                        }
                                    }
                                }
                            }

                            //se o processo caber no espaco disponivel na memória
                            if (piorEspaco != null)
                            {
                                for (int j = 0; j < framesNeeded; ++j)
                                {
                                    Frame newFrame = new Frame
                                    {
                                        IsInitial = false,
                                        RegB = espacosLivres[(int)piorEspaco].RegB + (memory.FramesSize * j),
                                        TipoAlg = Frame.TipoAlgVal.WorstFit,
                                        MemoryID = memory.ID,
                                        ProcessID = process.ID,
                                        Name = process.Name,
                                        FrameSize = (int)memory.FramesSize
                                    };

                                    newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(newFrame.RegB / memory.FramesSize) : 0;

                                    //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                    if (j + 1 == framesNeeded)
                                        newFrame.CapacidadeUtilizada = (int)(process.Size % memory.FramesSize);
                                    else
                                        newFrame.CapacidadeUtilizada = (int)memory.FramesSize;

                                    framesToInsert.Add(newFrame);
                                }

                                //se for necessario remove o item da lista de livre
                                if (espacosLivres[(int)piorEspaco].EspacosLivres - framesNeeded == 0)
                                {
                                    espacosLivres.RemoveAt((int)piorEspaco);
                                }
                                else
                                {
                                    //Atualiza a lista de espacos livres
                                    int quantidadeLivreAnt = espacosLivres[(int)piorEspaco].EspacosLivres;
                                    long regBAnt = espacosLivres[(int)piorEspaco].RegB;

                                    espacosLivres[(int)piorEspaco] = new EspacoLivre
                                    {                                        
                                        RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                        EspacosLivres = quantidadeLivreAnt - framesNeeded
                                    };
                                }

                                processInserted++;

                                watch.Stop();
                                // Get the elapsed time as a TimeSpan value.
                                TimeSpan ts = watch.Elapsed;

                                process.TimeToFindIndexWorst = ts.Milliseconds;
                            }
                        }            
                    }

                    switch (Alg) 
                    {
                        case "FirstFit":
                            memory.IsFirstFitCompleted = true;
                            memory.FirstFitInseridos = processInserted;
                            break;
                        case "NextFit":
                            memory.IsNextFitCompleted = true;
                            memory.NextFitInseridos = processInserted;
                            break;
                        case "BestFit":
                            memory.IsBestFitCompleted = true;
                            memory.BestFitInseridos = processInserted;
                            break;
                        case "WorstFit":
                            memory.IsWorstFitCompleted = true;
                            memory.WorstFitInseridos = processInserted;
                            break;
                    }

                    _context.Frames.AddRange(framesToInsert);
                    _context.Processes.UpdateRange(processToInsert);
                    _context.Memories.Update(memory);
                    _context.SaveChanges();

                    return Json(
                        new
                        {
                            processos = processInserted,
                            success = true
                        }
                    );
                }
                else
                {
                    throw new Exception("Mémoria não informada!");
                }
            }
            catch (Exception e)
            {
                return Json(
                    new
                    {
                        success = false,
                        error = e.Message
                    }
                );
            }
        }

        // GET: Simulations/Delete/5
        [RedirectAction]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id == null)
                return RedirectToAction("Error404", "Erros");

            var memory = await _context.Memories
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (memory == null)
                return RedirectToAction("Error404", "Erros");

            if (memory.UserID != HttpContext.Session.GetInt32("UserID"))
                return RedirectToAction("Error403", "Erros");

            return View(memory);
        }

        // POST: Simulations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            var memoriesFrames = await _context.Frames.Where(f => f.MemoryID == id).ToListAsync();
            var memoriesProcess = await _context.Processes.Where(p => p.MemoryID == id).ToListAsync();            
            var memory = await _context.Memories.FindAsync(id);

            _context.Frames.RemoveRange(memoriesFrames);
            _context.Processes.RemoveRange(memoriesProcess);
            _context.Memories.Remove(memory);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
