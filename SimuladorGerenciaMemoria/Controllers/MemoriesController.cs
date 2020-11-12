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
        public async Task<IActionResult> Create([Bind("ID,Name,SimulationID,Size,FramesSize,IsGeneratedProcessList,InitialState,UserID")] Memory memory)
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

            ScriptProcess scriptProcess = new ScriptProcess(memory, processesNeeded);

            List<Models.Process> processList = scriptProcess.CreateProcesses();
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
                    frameToAdd.FrameSize = (int) memory.FramesSize;

                    //se for o ultimo frame, verifica qual a capacidade utilizada do mesmo
                    if (i + 1 == framesNeeded)
                        frameToAdd.CapacidadeUtilizada = (int) (item.RegL % memory.FramesSize);
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

                if(_memory == null)
                    throw new Exception("Houve um erro! Memória não encontrada.");

                long _memoryToFeel = (long) (_memory.Size * ((float)MemoryToFeelPerc / 100));

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

        [HttpPost]
        //public JsonResult QuickFitInsertion(Process pProcess, List<KeyValuePair<long, long>> mappedMemory, Dictionary<long, bool> allRegB)
        public JsonResult QuickFitInsertion(int? id)
        {
            /*var framesNeeded = pProcess.RegL / this.FramesSize;
            framesNeeded = pProcess.RegL % this.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

            if (!mappedMemory.Any(a => a.Value >= framesNeeded))
                return -1;

            var placeToInsert = mappedMemory.Where(w => w.Value >= framesNeeded).FirstOrDefault().Key;
            var auxRegb = placeToInsert;

            for (long usedFrames = 1; usedFrames <= framesNeeded; usedFrames++, auxRegb += FramesSize)
                allRegB[auxRegb] = true;

            pProcess.TimeToFindIndex = 0;
            return Convert.ToInt32(placeToInsert);*/

            return Json(
                new
                {
                    success = false
                }
            );
        }

        [HttpPost]
        public JsonResult FirstFitInsertion(string id) 
        {
            Console.WriteLine("ID: "+ id);

            if (id != null) 
            {
                return Json(
                    new
                    {
                        success = true
                    }
                );
            }

            /*var framesNeeded = pProcess.RegL / this.FramesSize;
            framesNeeded = pProcess.RegL % this.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

            int indexToReturn = 0, auxIndex = 0;

            for (int i = 0; i < this.Frames.Count; ++i)
            {
                if (this.Frames[i].Process != null)
                    indexToReturn = 0;
                else
                {
                    if (indexToReturn == 0)
                        auxIndex = i;
                    indexToReturn++;
                }
                if (indexToReturn == framesNeeded)
                {
                    //pProcess.TimeToFindIndex = i;
                    return auxIndex;
                }
            }*/

            return Json(
                new
                {
                    success = false
                }
            );
        }

        [HttpPost]
        public JsonResult BestFitInsertion(int? id)
        {
            /*var framesNeeded = pProcess.RegL / this.FramesSize;
            framesNeeded = pProcess.RegL % this.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

            Dictionary<int, int> mapEmptyFrames = new Dictionary<int, int>();
            int emptyFrames = 0, auxIndex = 0;
            int i = 0;
            for (i = 0; i < this.Frames.Count; ++i)
            {
                if (this.Frames[i].Process != null)
                    emptyFrames = 0;
                else
                {
                    if (emptyFrames == 0)
                        auxIndex = i;
                    emptyFrames++;
                }
                if ((i == this.Frames.Count - 1 || this.Frames[i + 1].Process != null) && emptyFrames > 0)
                {
                    mapEmptyFrames.Add(auxIndex, emptyFrames);
                    if (emptyFrames == framesNeeded)
                    {
                        pProcess.TimeToFindIndex = i;
                        return auxIndex;
                    }
                }
            }
            if (mapEmptyFrames.Count(w => w.Value >= framesNeeded) == 0)
                return -1;

            var bestFit = mapEmptyFrames.Where(w => w.Value >= framesNeeded).OrderBy(o => o.Value).FirstOrDefault();
            pProcess.TimeToFindIndex = i;
            return bestFit.Key;*/

            return Json(
                new
                {
                    success = false
                }
            );
        }

        [HttpPost]
        public JsonResult WorstFitInsertion(int? id)
        {
            /*var framesNeeded = pProcess.RegL / this.FramesSize;
            framesNeeded = pProcess.RegL % this.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

            Dictionary<int, int> mapEmptyFrames = new Dictionary<int, int>();
            int emptyFrames = 0, auxIndex = 0;
            int i = 0;
            for (i = 0; i < this.Frames.Count; ++i)
            {
                if (this.Frames[i].Process != null)
                    emptyFrames = 0;
                else
                {
                    if (emptyFrames == 0)
                        auxIndex = i;
                    emptyFrames++;
                }
                if ((i == this.Frames.Count - 1 || this.Frames[i + 1].Process != null) && emptyFrames > 0)
                    mapEmptyFrames.Add(auxIndex, emptyFrames);
            }

            if (mapEmptyFrames.Count(w => w.Value >= framesNeeded) == 0)
                return -1;

            var worstFit = mapEmptyFrames.Where(w => w.Value >= framesNeeded).OrderByDescending(o => o.Value).FirstOrDefault();
            pProcess.TimeToFindIndex = i;
            return worstFit.Key;*/

            return Json(
                new
                {
                    success = false
                }
            );
        }        
    }
}
