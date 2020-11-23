using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;

namespace SimuladorGerenciaMemoria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SimuladorContext _context;

        public HomeController(SimuladorContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [RedirectAction]
        public IActionResult Index(int? id, string algs)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.isDashboard = true;
            Memory memory;
            int simulationSelected = 0;

            if (id != null)
            {
                memory = _context.Memories.Find(id);

                if (memory == null)
                    return RedirectToAction("Error404", "Erros");

                if (memory.UserID != HttpContext.Session.GetInt32("UserID"))
                    return RedirectToAction("Error403", "Erros");

                simulationSelected = memory.SimulationID;
            }

            SelectList simulations = new SelectList(_context.Simulations.
                Where(s => s.UserID == HttpContext.Session.GetInt32("UserID")).
                OrderByDescending(s => s.CreateDate),
                "ID", "Name");

            var selected = simulations.First();
            selected.Selected = true;

            ViewBag.SimulationID = simulations;

            if (id != null)
            {
                ViewBag.MemorySelected = id;
                ViewBag.SimulationSelected = simulationSelected;
            }

            if (algs != null)
            {
                if (algs != "all" && algs != "first" && algs != "next" && algs != "best" && algs != "worst")
                    return RedirectToAction("Error404", "Erros");
            }
            else{
                algs = "all";
            }

            ViewBag.Algs = algs;

            return View();
        }

        [HttpPost]
        public JsonResult GetMemories(int? simulationId) 
        {
            try
            {
                if (simulationId == null) 
                    throw new Exception("Simulação não encontrada!");

                Dictionary<int, string> memoriesMap = new Dictionary<int, string>();

                var memories = _context.Memories
                    .Where(m => m.SimulationID == simulationId &&
                    m.UserID == HttpContext.Session.GetInt32("UserID"))
                    .OrderByDescending(m => m.CreateDate);

                foreach (var item in memories) 
                    memoriesMap.Add(item.ID, item.Name);

                return Json(
                    new
                    {
                        memoriesMap = JsonConvert.SerializeObject(memoriesMap),
                        success = true
                    }
                );
            }
            catch (Exception e) 
            {
                return Json(
                    new
                    {
                        error = e.Message,
                        success = false
                    }
                );
            }
        }

        [HttpPost]
        public JsonResult GetMemoriesInicialDados(int? memoryID)
        {
            try
            {
                if (memoryID == null)
                    throw new Exception("Memória não informada!");

                Memory memory = _context.Memories.Find(memoryID);

                if (memory == null)
                    throw new Exception("Memória não encontrada!");

                var memoryProcessIniciais = _context.Processes
                    .Where(p => p.MemoryID == memoryID && p.isInitial == true);

                var processCount = memoryProcessIniciais.Count();
                var maiorProcess = memoryProcessIniciais.OrderByDescending(p => p.Size).First();
                var menorProcess = memoryProcessIniciais.OrderBy(p => p.Size).First();

                long processTamanhoSoma = 0;

                foreach (var item in memoryProcessIniciais) 
                    processTamanhoSoma += item.Size;

                var mediaProcess = processTamanhoSoma / processCount;

                var maiorProcessReturn = ((double) maiorProcess.Size / 1024); //em KiB
                var menorProcessReturn = ((double) menorProcess.Size / 1024); //em KiB
                var mediaProcessReturn = ((double) mediaProcess / 1024); //em KiB
                var memoriaTamanhoReturn = (((double) memory.Size / 1024) / 1024); //em MiB

                return Json(
                    new
                    {
                        isFirstCompleted = memory.IsFirstFitCompleted,
                        isNextCompleted = memory.IsNextFitCompleted,
                        isBestCompleted = memory.IsBestFitCompleted,
                        isWorstCompleted = memory.IsWorstFitCompleted,
                        maiorProcesso = maiorProcessReturn,
                        menorProcesso = menorProcessReturn,
                        mediaProcesso = mediaProcessReturn,
                        numeroProcessos = processCount,
                        memoriaTamanho = memoriaTamanhoReturn,
                        success = true
                    }
                );
            }
            catch (Exception e) 
            {
                return Json(
                    new
                    {
                        error = e.Message,
                        success = false
                    }
                );
            }
        }

        [HttpPost]
        public JsonResult GetMemoriesStatusInicial(int? memoryID)
        {
            try
            {
                //Map: int -> FrameNumber, int -> FrameID 
                Dictionary<int, int> mapFrameUsed = new Dictionary<int, int>();
                List<int> framesLivres = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                if (memoryID == null)
                    throw new Exception("Memória não informada!");

                Memory memory = _context.Memories.Find(memoryID);

                if (memory == null)
                    throw new Exception("Memória não encontrada!");

                var framesIniciais = _context.Frames
                    .Where(f => f.MemoryID == memoryID && f.IsInitial == true);

                long memoriaUsada = 0;
                long memoriaInutil = 0;

                foreach (var item in framesIniciais)
                {
                    mapFrameUsed.Add(item.FrameNumber, item.ID);

                    if (item.CapacidadeUtilizada != memory.FramesSize)
                        memoriaInutil += (memory.FramesSize - item.CapacidadeUtilizada);

                    memoriaUsada += memory.FramesSize;
                }

                long memoriaLivre = memory.Size - memoriaUsada;

                for (int i = 0; i < memory.FramesQTD; i++)
                {
                    //descobre a qual porcentagem da memoria o frame se encontra
                    double porc = Utils.Utils.RetornaPorcentagem(i, memory.FramesQTD);
                    int Index = Utils.Utils.RetornaIndex(porc);

                    if (!mapFrameUsed.ContainsKey(i)) framesLivres[Index] += 1;
                }

                return Json(
                    new
                    {                        
                        memoriaUtilizada = ((double)memoriaUsada / 1024), //KiB
                        memoriaLivre = ((double)memoriaLivre / 1024), //KiB
                        memoriaInutil = ((double)memoriaInutil / 1024), //KiB
                        framesLivres,
                        success = true
                    }
                );
            }
            catch (Exception e)
            {
                return Json(
                    new
                    {
                        error = e.Message,
                        success = false
                    }
                );
            }
        }

        [HttpPost]
        public JsonResult GetMemoriesAlgsData(int? memoryID, bool needFirst, bool needNext, bool needBest, bool needWorst)
        {
            try
            {
                if (memoryID == null) throw new Exception("Memória não informada!");

                Memory memory = _context.Memories.Find(memoryID);

                if(memory == null) throw new Exception("Memória não encontrada!");

                int first_processo_inserido = 0, first_menor_insercao = 0,
                    first_maior_insercao = 0, next_processo_inserido = 0,
                    next_menor_insercao = 0, next_maior_insercao = 0,
                    best_processo_inserido = 0, best_menor_insercao = 0,
                    worst_processo_inserido = 0, worst_menor_insercao = 0,
                    worst_maior_insercao = 0, best_maior_insercao = 0, processTotalCount = 0;

                double first_media_insercao = 0, next_media_insercao = 0, first_menor_process = 0, first_maior_process = 0,
                    best_media_insercao = 0, worst_media_insercao = 0, next_menor_process = 0, next_maior_process = 0,
                     best_menor_process = 0, best_maior_process = 0, worst_menor_process = 0, worst_maior_process = 0;

                var processInserted = _context.Processes
                    .Where(
                        p => p.MemoryID == memoryID
                        &&
                        p.isInitial == false
                    ).ToList();

                processTotalCount = processInserted.Count();

                var framesMemoria = _context.Frames
                        .Where(f => f.MemoryID == memoryID).ToList();

                if (needFirst && memory.IsFirstFitCompleted) 
                {
                    var framesMemoriaFirst = framesMemoria
                        .Where(f => f.TipoAlg == Frame.TipoAlgVal.FirstFit 
                        || f.IsInitial == true)
                        .ToList();

                    first_processo_inserido = memory.FirstFitInseridos;

                    var menor_insercao = processInserted.Where(p => p.TimeToFindIndexFirst != null).OrderBy(p => p.TimeToFindIndexFirst).First();
                    var maior_insercao = processInserted.Where(p => p.TimeToFindIndexFirst != null).OrderByDescending(p => p.TimeToFindIndexFirst).First();
                    var menor_process = processInserted.Where(p => p.TimeToFindIndexFirst != null).OrderBy(p => p.Size).First();
                    var maior_process = processInserted.Where(p => p.TimeToFindIndexFirst != null).OrderByDescending(p => p.Size).First();

                    first_menor_insercao = (int) menor_insercao.TimeToFindIndexFirst;
                    first_maior_insercao = (int) maior_insercao.TimeToFindIndexFirst;
                    first_menor_process = (double)menor_process.Size / 1024;
                    first_maior_process = (double)maior_process.Size / 1024;

                    System.Nullable<Double> mediaTiming =
                    (from Process in processInserted
                     select Process.TimeToFindIndexFirst)
                    .Average();

                    first_media_insercao = (double) mediaTiming;
                }

                if (needNext && memory.IsNextFitCompleted)
                {
                    var framesMemoriaNext = framesMemoria
                        .Where(f => f.TipoAlg == Frame.TipoAlgVal.NextFit
                        || f.IsInitial == true)
                        .ToList();

                    next_processo_inserido = memory.NextFitInseridos;

                    var menor_insercao = processInserted.Where(p => p.TimeToFindIndexNext != null).OrderBy(p => p.TimeToFindIndexNext).First();
                    var maior_insercao = processInserted.Where(p => p.TimeToFindIndexNext != null).OrderByDescending(p => p.TimeToFindIndexNext).First();
                    var menor_process = processInserted.Where(p =>  p.TimeToFindIndexNext != null).OrderBy(p => p.Size).First();
                    var maior_process = processInserted.Where(p =>  p.TimeToFindIndexNext != null).OrderByDescending(p => p.Size).First();

                    next_menor_insercao = (int)menor_insercao.TimeToFindIndexNext;
                    next_maior_insercao = (int)maior_insercao.TimeToFindIndexNext;
                    next_menor_process = (double)menor_process.Size / 1024;
                    next_maior_process = (double)maior_process.Size / 1024;

                    System.Nullable<Double> mediaTiming =
                    (from Process in processInserted
                     select Process.TimeToFindIndexNext)
                    .Average();

                    next_media_insercao = (double)mediaTiming;
                }

                if (needBest && memory.IsBestFitCompleted)
                {
                    var framesMemoriaBest = framesMemoria
                        .Where(f => f.TipoAlg == Frame.TipoAlgVal.BestFit
                        || f.IsInitial == true)
                        .ToList();

                    best_processo_inserido = memory.BestFitInseridos;

                    var menor_insercao = processInserted.Where(p => p.TimeToFindIndexBest != null).OrderBy(p => p.TimeToFindIndexBest).First();
                    var maior_insercao = processInserted.Where(p => p.TimeToFindIndexBest != null).OrderByDescending(p => p.TimeToFindIndexBest).First();
                    var menor_process = processInserted.Where(p =>  p.TimeToFindIndexBest != null).OrderBy(p => p.Size).First();
                    var maior_process = processInserted.Where(p =>  p.TimeToFindIndexBest != null).OrderByDescending(p => p.Size).First();

                    best_menor_insercao = (int)menor_insercao.TimeToFindIndexBest;
                    best_maior_insercao = (int)maior_insercao.TimeToFindIndexBest;
                    best_menor_process = (double)menor_process.Size / 1024;
                    best_maior_process = (double)maior_process.Size / 1024;

                    System.Nullable<Double> mediaTiming =
                    (from Process in processInserted
                     select Process.TimeToFindIndexBest)
                    .Average();

                    best_media_insercao = (double)mediaTiming;
                }

                if (needWorst && memory.IsWorstFitCompleted)
                {
                    var framesMemoriaWorst = framesMemoria
                        .Where(f => f.TipoAlg == Frame.TipoAlgVal.WorstFit
                        || f.IsInitial == true)
                        .ToList();

                    worst_processo_inserido = memory.WorstFitInseridos;

                    var menor_insercao = processInserted.Where(p => p.TimeToFindIndexWorst != null).OrderBy(p => p.TimeToFindIndexWorst).First();
                    var maior_insercao = processInserted.Where(p => p.TimeToFindIndexWorst != null).OrderByDescending(p => p.TimeToFindIndexWorst).First();
                    var menor_process = processInserted.Where(p => p.TimeToFindIndexWorst != null).OrderBy(p => p.Size).First();
                    var maior_process = processInserted.Where(p => p.TimeToFindIndexWorst != null).OrderByDescending(p => p.Size).First();

                    worst_menor_insercao = (int)menor_insercao.TimeToFindIndexWorst;
                    worst_maior_insercao = (int)maior_insercao.TimeToFindIndexWorst;
                    worst_menor_process = (double)menor_process.Size / 1024;
                    worst_maior_process = (double)maior_process.Size / 1024;

                    System.Nullable<Double> mediaTiming =
                    (from Process in processInserted
                     select Process.TimeToFindIndexWorst)
                    .Average();

                    worst_media_insercao = (double)mediaTiming;
                }

                return Json(
                    new
                    {
                        processTotalCount,

                        first_processo_inserido,
                        first_menor_process,
                        first_maior_process,
                        first_menor_insercao,
                        first_media_insercao,
                        first_maior_insercao,

                        next_processo_inserido,
                        next_menor_process,
                        next_maior_process,
                        next_menor_insercao,
                        next_media_insercao,
                        next_maior_insercao,

                        best_processo_inserido,
                        best_menor_process,
                        best_maior_process,
                        best_menor_insercao,
                        best_media_insercao,
                        best_maior_insercao,

                        worst_processo_inserido,
                        worst_menor_process,
                        worst_maior_process,
                        worst_menor_insercao,
                        worst_media_insercao,
                        worst_maior_insercao,

                        success = true
                    }
                );
            }
            catch (Exception e) 
            {
                return Json(
                    new
                    {
                        error = e.Message,
                        success = false
                    }
                );
            }
        }

        [HttpPost]
        public JsonResult GetMemoriesAlgsGraficos(int? memoryID, bool needFirst, bool needNext, bool needBest, bool needWorst)
        {
            try
            {
                long memoriaUsadaFirst = 0, memoriaInutilFirst = 0, memoriaLivreFirst = 0,
                    memoriaUsadaNext = 0, memoriaInutilNext = 0, memoriaLivreNext = 0,
                    memoriaUsadaBest = 0, memoriaInutilBest = 0, memoriaLivreBest = 0,
                    memoriaUsadaWorst = 0, memoriaInutilWorst = 0, memoriaLivreWorst = 0;

                //Map: int -> FrameNumber, int -> FrameID 
                Dictionary<int, int> mapFrameUsedFirst = new Dictionary<int, int>();
                Dictionary<int, int> mapFrameUsedNext = new Dictionary<int, int>();
                Dictionary<int, int> mapFrameUsedBest = new Dictionary<int, int>();
                Dictionary<int, int> mapFrameUsedWorst = new Dictionary<int, int>();
                List<int> framesLivresFirst = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<int> framesLivresNext = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<int> framesLivresBest = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<int> framesLivresWorst = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                if (memoryID == null)
                    throw new Exception("Memória não informada!");

                Memory memory = _context.Memories.Find(memoryID);

                if (memory == null)
                    throw new Exception("Memória não encontrada!");

                var frames = _context.Frames
                    .Where(f => f.MemoryID == memoryID).ToList();

                if (needFirst) 
                {
                    var framesFirst = frames.Where(f => f.IsInitial == true
                    || f.TipoAlg == Frame.TipoAlgVal.FirstFit).ToList();

                    memoriaUsadaFirst = 0;
                    memoriaInutilFirst = 0;

                    foreach (var item in framesFirst)
                    {
                        mapFrameUsedFirst.Add(item.FrameNumber, item.ID);

                        if (item.CapacidadeUtilizada != memory.FramesSize)
                            memoriaInutilFirst += (memory.FramesSize - item.CapacidadeUtilizada);

                        memoriaUsadaFirst += memory.FramesSize;
                    }

                    memoriaLivreFirst = memory.Size - memoriaUsadaFirst;                    
                }

                if (needNext)
                {
                    var framesNext = frames.Where(f => f.IsInitial == true
                    || f.TipoAlg == Frame.TipoAlgVal.NextFit).ToList();

                    memoriaUsadaNext = 0;
                    memoriaInutilNext = 0;

                    foreach (var item in framesNext)
                    {
                        mapFrameUsedNext.Add(item.FrameNumber, item.ID);

                        if (item.CapacidadeUtilizada != memory.FramesSize)
                            memoriaInutilNext += (memory.FramesSize - item.CapacidadeUtilizada);

                        memoriaUsadaNext += memory.FramesSize;
                    }

                    memoriaLivreNext = memory.Size - memoriaUsadaNext;                   
                }

                if (needBest)
                {
                    var framesBest = frames.Where(f => f.IsInitial == true
                    || f.TipoAlg == Frame.TipoAlgVal.BestFit).ToList();

                    memoriaUsadaBest = 0;
                    memoriaInutilBest = 0;

                    foreach (var item in framesBest)
                    {
                        mapFrameUsedBest.Add(item.FrameNumber, item.ID);

                        if (item.CapacidadeUtilizada != memory.FramesSize)
                            memoriaInutilBest += (memory.FramesSize - item.CapacidadeUtilizada);

                        memoriaUsadaBest += memory.FramesSize;
                    }

                    memoriaLivreBest = memory.Size - memoriaUsadaBest;                    
                }

                if (needWorst)
                {
                    var framesWorst = frames.Where(f => f.IsInitial == true
                    || f.TipoAlg == Frame.TipoAlgVal.WorstFit).ToList();

                    memoriaUsadaWorst = 0;
                    memoriaInutilWorst = 0;

                    foreach (var item in framesWorst)
                    {
                        mapFrameUsedWorst.Add(item.FrameNumber, item.ID);

                        if (item.CapacidadeUtilizada != memory.FramesSize)
                            memoriaInutilWorst += (memory.FramesSize - item.CapacidadeUtilizada);

                        memoriaUsadaWorst += memory.FramesSize;
                    }

                    memoriaLivreWorst = memory.Size - memoriaUsadaWorst;
                }

                for (int i = 0; i < memory.FramesQTD; i++)
                {
                    //descobre a qual porcentagem da memoria o frame se encontra
                    double porc = Utils.Utils.RetornaPorcentagem(i, memory.FramesQTD);
                    int index = Utils.Utils.RetornaIndex(porc);

                    if (needFirst) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[index] += 1;
                    if (needNext) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[index] += 1;
                    if (needBest) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[index] += 1;
                    if (needWorst) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[index] += 1;                 
                }

                return Json(
                    new
                    {
                        memoriaUtilizadaFirst = ((double)memoriaUsadaFirst / 1024), //KiB
                        memoriaLivreFirst = ((double)memoriaLivreFirst / 1024), //KiB
                        memoriaInutilFirst = ((double)memoriaInutilFirst / 1024), //KiB
                        framesLivresFirst,

                        memoriaUtilizadaNext = ((double)memoriaUsadaNext / 1024), //KiB
                        memoriaLivreNext = ((double)memoriaLivreNext / 1024), //KiB
                        memoriaInutilNext = ((double)memoriaInutilNext / 1024), //KiB
                        framesLivresNext,

                        memoriaUtilizadaBest = ((double)memoriaUsadaBest / 1024), //KiB
                        memoriaLivreBest = ((double)memoriaLivreBest / 1024), //KiB
                        memoriaInutilBest = ((double)memoriaInutilBest / 1024), //KiB
                        framesLivresBest,

                        memoriaUtilizadaWorst = ((double)memoriaUsadaWorst / 1024), //KiB
                        memoriaLivreWorst = ((double)memoriaLivreWorst / 1024), //KiB
                        memoriaInutilWorst = ((double)memoriaInutilWorst / 1024), //KiB
                        framesLivresWorst,

                        success = true
                    }
                );
            }
            catch (Exception e)
            {
                return Json(
                    new
                    {
                        error = e.Message,
                        success = false
                    }
                );
            }
        }

        [HttpPost]
        public JsonResult GetComparacoes(int? memoryID, bool needFirst, bool needNext, bool needBest, bool needWorst)
        {
            try
            {
                //Fragmentacao Interna
                List<double> fragmentacaoInternaFirst = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<double> fragmentacaoInternaNext = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<double> fragmentacaoInternaBest = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<double> fragmentacaoInternaWorst = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                //int -> frameNumber, long -> bytes
                Dictionary<int, long> mapUsedCapacityFirst = new Dictionary<int, long>();
                Dictionary<int, long> mapUsedCapacityNext = new Dictionary<int, long>();
                Dictionary<int, long> mapUsedCapacityBest = new Dictionary<int, long>();
                Dictionary<int, long> mapUsedCapacityWorst = new Dictionary<int, long>();

                //Tempo insercao medio por distribuição do total de pocessos inseridos
                //lists to return
                List<double> tempoInsercaoFirst = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<double> tempoInsercaoNext = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<double> tempoInsercaoBest = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                List<double> tempoInsercaoWorst = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                //Maps para guardar o total de (ms) gastos por processo
                //int -> index vet, int -> total (ms)
                Dictionary<int, int> mapTOTinsertedFirst = new Dictionary<int, int>();
                Dictionary<int, int> mapTOTinsertedNext = new Dictionary<int, int>();
                Dictionary<int, int> mapTOTinsertedBest = new Dictionary<int, int>();
                Dictionary<int, int> mapTOTinsertedWorst = new Dictionary<int, int>();

                List<Frame> framesFirst = new List<Frame>();
                List<Frame> framesBest = new List<Frame>();
                List<Frame> framesNext = new List<Frame>();
                List<Frame> framesWorst = new List<Frame>();

                List<Models.Process> processesInsertedFirst = new List<Models.Process>();
                List<Models.Process> processesInsertedNext = new List<Models.Process>();
                List<Models.Process> processesInsertedBest = new List<Models.Process>();
                List<Models.Process> processesInsertedWorst = new List<Models.Process>();

                if (memoryID == null)
                    throw new Exception("Memória não informada!");

                Memory memory = _context.Memories.Find(memoryID);

                if (memory == null)
                    throw new Exception("Memória não encontrada!");

                var frames = _context.Frames
                    .Where(f => f.MemoryID == memoryID).ToList();

                List<Models.Process> processesInserted = _context.Processes.Where(p => p.MemoryID == memory.ID && p.isInitial == false).OrderBy(p => p.ID).ToList();

                if (needFirst && memory.IsFirstFitCompleted) 
                {
                    framesFirst = frames.Where(f => f.IsInitial == true
                    || f.TipoAlg == Frame.TipoAlgVal.FirstFit
                    && f.CapacidadeUtilizada != memory.FramesSize).ToList();

                    foreach (var item in framesFirst) mapUsedCapacityFirst.Add(item.FrameNumber, item.CapacidadeUtilizada);
                    processesInsertedFirst = processesInserted.Where(p => p.TimeToFindIndexFirst != null).ToList();
                }

                if (needNext && memory.IsNextFitCompleted)
                {
                    framesNext = frames.Where(f => f.IsInitial == true
                    || f.TipoAlg == Frame.TipoAlgVal.NextFit
                    && f.CapacidadeUtilizada != memory.FramesSize).ToList();

                    foreach (var item in framesNext) mapUsedCapacityNext.Add(item.FrameNumber, item.CapacidadeUtilizada);
                    processesInsertedNext = processesInserted.Where(p => p.TimeToFindIndexNext != null).ToList();
                }

                if (needBest && memory.IsBestFitCompleted)
                {
                    framesBest = frames.Where(f => f.IsInitial == true
                    || f.TipoAlg == Frame.TipoAlgVal.BestFit
                    && f.CapacidadeUtilizada != memory.FramesSize).ToList();

                    foreach (var item in framesBest) mapUsedCapacityBest.Add(item.FrameNumber, item.CapacidadeUtilizada);
                    processesInsertedBest = processesInserted.Where(p => p.TimeToFindIndexBest != null).ToList();
                }

                if (needWorst && memory.IsWorstFitCompleted)
                {
                   framesWorst = frames.Where(f => f.IsInitial == true
                   || f.TipoAlg == Frame.TipoAlgVal.WorstFit
                   && f.CapacidadeUtilizada != memory.FramesSize).ToList();

                    foreach (var item in framesWorst) mapUsedCapacityWorst.Add(item.FrameNumber, item.CapacidadeUtilizada);
                    processesInsertedWorst = processesInserted.Where(p => p.TimeToFindIndexWorst != null).ToList();
                }

                int i = 0;

                for (i = 0; i < memory.FramesQTD; i++)
                {
                    //descobre a qual porcentagem da memoria o frame se encontra
                    double porc = Utils.Utils.RetornaPorcentagem(i, memory.FramesQTD);
                    int index = Utils.Utils.RetornaIndex(porc);

                    if (needFirst && memory.IsFirstFitCompleted)
                        if (mapUsedCapacityFirst.ContainsKey(i)) fragmentacaoInternaFirst[index] += ((double)mapUsedCapacityFirst[i] / 1024);

                    if (needNext && memory.IsNextFitCompleted)
                        if (mapUsedCapacityNext.ContainsKey(i)) fragmentacaoInternaNext[index] += ((double)mapUsedCapacityNext[i] / 1024);

                    if (needBest && memory.IsBestFitCompleted)
                        if (mapUsedCapacityBest.ContainsKey(i)) fragmentacaoInternaBest[index] += ((double)mapUsedCapacityBest[i] / 1024);

                    if (needWorst && memory.IsWorstFitCompleted)
                        if (mapUsedCapacityWorst.ContainsKey(i)) fragmentacaoInternaWorst[index] += ((double)mapUsedCapacityWorst[i] / 1024);
                }

                if (needFirst && memory.IsFirstFitCompleted)
                {
                    i = 0;
                    foreach (var item in processesInsertedFirst)
                    {
                        int porc = Utils.Utils.RetornaPorcentagem(i, processesInsertedFirst.Count());
                        int index = Utils.Utils.RetornaIndex(porc);

                        if (mapTOTinsertedFirst.ContainsKey(index))
                        {
                            int valToUpdate = mapTOTinsertedFirst[index];
                            mapTOTinsertedFirst.Remove(index);
                            mapTOTinsertedFirst.Add(index, valToUpdate + (int)item.TimeToFindIndexFirst);
                        }
                        else mapTOTinsertedFirst.Add(index, (int)item.TimeToFindIndexFirst);

                        i++;
                    }
                }

                if (needNext && memory.IsNextFitCompleted)
                {
                    i = 0;
                    foreach (var item in processesInsertedNext)
                    {
                        int porc = Utils.Utils.RetornaPorcentagem(i, processesInsertedNext.Count());
                        int index = Utils.Utils.RetornaIndex(porc);

                        if (mapTOTinsertedNext.ContainsKey(index))
                        {
                            int valToUpdate = mapTOTinsertedNext[index];
                            mapTOTinsertedNext.Remove(index);
                            mapTOTinsertedNext.Add(index, valToUpdate + (int)item.TimeToFindIndexNext);
                        }
                        else mapTOTinsertedNext.Add(index, (int)item.TimeToFindIndexNext);

                        i++;
                    }
                }                    

                if (needBest && memory.IsBestFitCompleted)
                {
                    i = 0;
                    foreach (var item in processesInsertedBest)
                    {
                        int porc = Utils.Utils.RetornaPorcentagem(i, processesInsertedBest.Count());
                        int index = Utils.Utils.RetornaIndex(porc);

                        if (mapTOTinsertedBest.ContainsKey(index))
                        {
                            int valToUpdate = mapTOTinsertedBest[index];
                            mapTOTinsertedBest.Remove(index);
                            mapTOTinsertedBest.Add(index, valToUpdate + (int)item.TimeToFindIndexBest);
                        }
                        else mapTOTinsertedBest.Add(index, (int)item.TimeToFindIndexBest);

                        i++;
                    }
                }                    

                if (needWorst && memory.IsWorstFitCompleted)
                {
                    i = 0;
                    foreach (var item in processesInsertedWorst)
                    {
                        int porc = Utils.Utils.RetornaPorcentagem(i, processesInsertedWorst.Count());
                        int index = Utils.Utils.RetornaIndex(porc);

                        if (mapTOTinsertedWorst.ContainsKey(index))
                        {
                            int valToUpdate = mapTOTinsertedWorst[index];
                            mapTOTinsertedWorst.Remove(index);
                            mapTOTinsertedWorst.Add(index, valToUpdate + (int)item.TimeToFindIndexWorst);
                        }
                        else mapTOTinsertedWorst.Add(index, (int)item.TimeToFindIndexWorst);

                        i++;
                    }
                }

                for (i = 0; i < 10; i++) 
                {
                    if (needFirst && memory.IsFirstFitCompleted)
                        tempoInsercaoFirst[i] = (double)(mapTOTinsertedFirst[i] / (double)(processesInsertedFirst.Count() * 0.10));

                    if (needNext && memory.IsNextFitCompleted)
                        tempoInsercaoNext[i] = (double)(mapTOTinsertedNext[i] / (double)(processesInsertedNext.Count() * 0.10));

                    if (needBest && memory.IsBestFitCompleted)
                        tempoInsercaoBest[i] = (double)(mapTOTinsertedBest[i] / (double)(processesInsertedBest.Count() * 0.10));

                    if (needWorst && memory.IsWorstFitCompleted)
                        tempoInsercaoWorst[i] = (double)(mapTOTinsertedWorst[i] / (double)(processesInsertedWorst.Count() * 0.10));
                }

                return Json(
                    new
                    {
                        fragmentacaoInternaFirst,
                        fragmentacaoInternaNext,
                        fragmentacaoInternaBest,
                        fragmentacaoInternaWorst,
                        tempoInsercaoFirst,
                        tempoInsercaoNext,
                        tempoInsercaoBest,
                        tempoInsercaoWorst,
                        frameSize = memory.FramesSize,
                        success = true
                    }
                );
            }
            catch (Exception e)
            {
                return Json(
                    new
                    {
                        error = e.Message,
                        success = false
                    }
                );
            }
        }

                [RedirectAction]
        public IActionResult About()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
