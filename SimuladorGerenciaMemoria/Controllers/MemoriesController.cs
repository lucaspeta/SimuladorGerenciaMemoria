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

            return View(memory);
        }

        // GET: Memories/Create
        [RedirectAction]
        public IActionResult Create()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.SimulationID = new SelectList(_context.Simulations, "ID", "Name");
            ViewBag.UserID = HttpContext.Session.GetInt32("UserID");

            return View();
        }

        // POST: Memories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> Create([Bind("ID,Name,SimulationID,Size,FramesSize,IsGeneratedProcessList,InitialState,UserID,InitialProcessMin,InitialProcessMax")] Memory memory)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            memory.Size = memory.Size * 1024; //transfoma em bytes
            memory.CreateDate = DateTime.Now;
            memory.FramesQTD = memory.Size / memory.FramesSize;

            int initialState = 25;

            switch (memory.InitialState)
            {
                case Memory.InitialStatePickList.Pequeno:
                    initialState = 25;
                    break;
                case Memory.InitialStatePickList.Medio:
                    initialState = 50;
                    break;
                case Memory.InitialStatePickList.Grande:
                    initialState = 75;
                    break;
            }

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
                    frameToAdd.FrameNumber = frameToAdd.RegB > 0 ? (int)(memory.Size / frameToAdd.RegB) : 0;
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

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,CreateDate,Size,FramesSize,FramesQTD,SimulationID,IsGeneratedProcessList,UserID")] Memory memory)
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

            switch (_memory.InitialState)
            {
                case Memory.InitialStatePickList.Pequeno:
                    ViewBag.MaximoPossivel = 99 - 25;
                    break;
                case Memory.InitialStatePickList.Medio:
                    ViewBag.MaximoPossivel = 99 - 50;
                    break;
                case Memory.InitialStatePickList.Grande:
                    ViewBag.MaximoPossivel = 99 - 75;
                    break;
            }

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
                Memory _memory = _context.Memories.Find(memoryID);

                if (_memory == null)
                    throw new Exception("Houve um erro! Memória não encontrada.");

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

            //index na lista de livres
            int espacoLivresIndex = 0;

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
                            Index = espacoLivresIndex,
                            EspacosLivres = framesLivres,
                            RegB = regB
                        };

                        listaEspacoLivre.Add(espacoLivre);

                        //reseta a quantidade de frames livres
                        framesLivres = 0;

                        //soma o index na lista 
                        espacoLivresIndex++;
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
                    List<Process> processToInsert = GetProcessesToInsert(id);
                    List<Frame> initialFrames = GetInitialFrames(id);
                    List<Frame> framesToInsert = new List<Frame>();
                    List<EspacoLivre> espacosLivres = GeraListaLivres(memory, initialFrames);

                    int processInserted = 0;

                    foreach (var process in processToInsert)
                    {
                        // tempo de execução para acha o index do processo
                        var watch = System.Diagnostics.Stopwatch.StartNew();

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

                                        newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(memory.Size / newFrame.RegB) : 0;

                                        //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                        if (j + 1 == framesNeeded)
                                        {
                                            newFrame.CapacidadeUtilizada = (int)(process.RegL % memory.FramesSize);
                                        }
                                        else
                                        {
                                            newFrame.CapacidadeUtilizada = (int)memory.FramesSize;
                                        }
                                    }

                                    //se for necessario remove o item da lista de livre
                                    if (espacosLivres[i].EspacosLivres - framesNeeded == 0)
                                    {
                                        espacosLivres.RemoveAt(i);
                                    }
                                    else
                                    {
                                        //Atualiza a lista de espacos livres
                                        int quantidadeLivreAnt = espacosLivres[i].EspacosLivres;
                                        long regBAnt = espacosLivres[i].RegB;

                                        espacosLivres.RemoveAt(i);

                                        espacosLivres[i] = new EspacoLivre
                                        {
                                            Index = i,
                                            RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                            EspacosLivres = quantidadeLivreAnt - framesNeeded
                                        };
                                    }

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
                                            TipoAlg = Frame.TipoAlgVal.FirstFit,
                                            MemoryID = memory.ID,
                                            ProcessID = process.ID,
                                            Name = process.Name,
                                            FrameSize = (int)memory.FramesSize
                                        };

                                        newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(memory.Size / newFrame.RegB) : 0;

                                        //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                        if (j + 1 == framesNeeded)
                                        {
                                            newFrame.CapacidadeUtilizada = (int)(process.RegL % memory.FramesSize);
                                        }
                                        else
                                        {
                                            newFrame.CapacidadeUtilizada = (int)memory.FramesSize;
                                        }
                                    }

                                    //se for necessario remove o item da lista de livre
                                    if (espacosLivres[i].EspacosLivres - framesNeeded == 0)
                                    {
                                        espacosLivres.RemoveAt(i);
                                    }
                                    else
                                    {
                                        //Atualiza a lista de espacos livres
                                        int quantidadeLivreAnt = espacosLivres[i].EspacosLivres;
                                        long regBAnt = espacosLivres[i].RegB;

                                        espacosLivres.RemoveAt(i);

                                        espacosLivres[i] = new EspacoLivre
                                        {
                                            Index = i,
                                            RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                            EspacosLivres = quantidadeLivreAnt - framesNeeded
                                        };
                                    }

                                    processInserted++;
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
                                    {
                                        melhorEspaco = espacosLivres[i].Index;
                                    }
                                    else 
                                    {
                                        if (espacosLivres[(int)melhorEspaco].EspacosLivres > espacosLivres[i].EspacosLivres) 
                                        {
                                            melhorEspaco = espacosLivres[i].Index;
                                        }                                        
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
                                        TipoAlg = Frame.TipoAlgVal.FirstFit,
                                        MemoryID = memory.ID,
                                        ProcessID = process.ID,
                                        Name = process.Name,
                                        FrameSize = (int)memory.FramesSize
                                    };

                                    newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(memory.Size / newFrame.RegB) : 0;

                                    //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                    if (j + 1 == framesNeeded)
                                    {
                                        newFrame.CapacidadeUtilizada = (int)(process.RegL % memory.FramesSize);
                                    }
                                    else
                                    {
                                        newFrame.CapacidadeUtilizada = (int)memory.FramesSize;
                                    }
                                }

                                //se for necessario remove o item da lista de livre
                                if (espacosLivres[(int)melhorEspaco].EspacosLivres - framesNeeded == 0)
                                {
                                    espacosLivres.RemoveAt((int)melhorEspaco);
                                }
                                else
                                {
                                    //Atualiza a lista de espacos livres
                                    int quantidadeLivreAnt = espacosLivres[(int)melhorEspaco].EspacosLivres;
                                    long regBAnt = espacosLivres[(int)melhorEspaco].RegB;

                                    espacosLivres.RemoveAt((int)melhorEspaco);

                                    espacosLivres[(int)melhorEspaco] = new EspacoLivre
                                    {
                                        Index = (int)melhorEspaco,
                                        RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                        EspacosLivres = quantidadeLivreAnt - framesNeeded
                                    };
                                }

                                processInserted++;
                                break;
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
                                    {
                                        piorEspaco = espacosLivres[i].Index;
                                    }
                                    else
                                    {
                                        if (espacosLivres[(int)piorEspaco].EspacosLivres < espacosLivres[i].EspacosLivres)
                                        {
                                            piorEspaco = espacosLivres[i].Index;
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
                                        TipoAlg = Frame.TipoAlgVal.FirstFit,
                                        MemoryID = memory.ID,
                                        ProcessID = process.ID,
                                        Name = process.Name,
                                        FrameSize = (int)memory.FramesSize
                                    };

                                    newFrame.FrameNumber = newFrame.RegB > 0 ? (int)(memory.Size / newFrame.RegB) : 0;

                                    //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                                    if (j + 1 == framesNeeded)
                                    {
                                        newFrame.CapacidadeUtilizada = (int)(process.RegL % memory.FramesSize);
                                    }
                                    else
                                    {
                                        newFrame.CapacidadeUtilizada = (int)memory.FramesSize;
                                    }
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

                                    espacosLivres.RemoveAt((int)piorEspaco);

                                    espacosLivres[(int)piorEspaco] = new EspacoLivre
                                    {
                                        Index = (int)piorEspaco,
                                        RegB = regBAnt + (memory.FramesSize * framesNeeded),
                                        EspacosLivres = quantidadeLivreAnt - framesNeeded
                                    };
                                }

                                processInserted++;
                                watch.Stop();

                                var elepsedMS = watch.ElapsedMilliseconds;
                                process.TimeToFindIndex = elepsedMS;
                                break;
                            }
                        }                        
                    }

                    _context.Frames.AddRange(framesToInsert);
                    _context.Processes.UpdateRange(processToInsert);
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
    }
}
