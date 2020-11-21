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
        public IActionResult Index(int? id)
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

                if(memory.UserID != HttpContext.Session.GetInt32("UserID"))
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
                    double porc = ((i * 100) / memory.FramesQTD);

                    if (needFirst)
                    {
                        if (porc <= 10) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[0] += 1;
                        if (porc <= 20 && porc > 10) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[1] += 1;
                        if (porc <= 30 && porc > 20) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[2] += 1;
                        if (porc <= 40 && porc > 30) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[3] += 1;
                        if (porc <= 50 && porc > 40) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[4] += 1;
                        if (porc <= 60 && porc > 50) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[5] += 1;
                        if (porc <= 70 && porc > 60) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[6] += 1;
                        if (porc <= 80 && porc > 70) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[7] += 1;
                        if (porc <= 90 && porc > 80) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[8] += 1;
                        if (porc <= 100 && porc > 90) if (!mapFrameUsedFirst.ContainsKey(i)) framesLivresFirst[9] += 1;
                    }

                    if (needNext)
                    {
                        if (porc <= 10) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[0] += 1;
                        if (porc <= 20 && porc > 10) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[1] += 1;
                        if (porc <= 30 && porc > 20) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[2] += 1;
                        if (porc <= 40 && porc > 30) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[3] += 1;
                        if (porc <= 50 && porc > 40) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[4] += 1;
                        if (porc <= 60 && porc > 50) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[5] += 1;
                        if (porc <= 70 && porc > 60) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[6] += 1;
                        if (porc <= 80 && porc > 70) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[7] += 1;
                        if (porc <= 90 && porc > 80) if (!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[8] += 1;
                        if (porc <= 100 && porc > 90) if(!mapFrameUsedNext.ContainsKey(i)) framesLivresNext[9] += 1;
                    }

                    if (needBest)
                    {
                        if (porc <= 10) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[0] += 1;
                        if (porc <= 20 && porc > 10) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[1] += 1;
                        if (porc <= 30 && porc > 20) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[2] += 1;
                        if (porc <= 40 && porc > 30) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[3] += 1;
                        if (porc <= 50 && porc > 40) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[4] += 1;
                        if (porc <= 60 && porc > 50) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[5] += 1;
                        if (porc <= 70 && porc > 60) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[6] += 1;
                        if (porc <= 80 && porc > 70) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[7] += 1;
                        if (porc <= 90 && porc > 80) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[8] += 1;
                        if (porc <= 100 && porc > 90) if (!mapFrameUsedBest.ContainsKey(i)) framesLivresBest[9] += 1;
                    }

                    if (needWorst) 
                    {
                        if (porc <= 10) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[0] += 1;
                        if (porc <= 20 && porc > 10) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[1] += 1;
                        if (porc <= 30 && porc > 20) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[2] += 1;
                        if (porc <= 40 && porc > 30) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[3] += 1;
                        if (porc <= 50 && porc > 40) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[4] += 1;
                        if (porc <= 60 && porc > 50) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[5] += 1;
                        if (porc <= 70 && porc > 60) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[6] += 1;
                        if (porc <= 80 && porc > 70) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[7] += 1;
                        if (porc <= 90 && porc > 80) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[8] += 1;
                        if (porc <= 100 && porc > 90) if (!mapFrameUsedWorst.ContainsKey(i)) framesLivresWorst[9] += 1;
                    }                   
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
