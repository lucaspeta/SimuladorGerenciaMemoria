using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;

namespace SimuladorGerenciaMemoria.Controllers
{
    public class SimulationsController : Controller
    {
        private readonly SimuladorContext _context;

        public SimulationsController(SimuladorContext context)
        {
            _context = context;
        }

        // GET: Simulations
        [RedirectAction]
        public async Task<IActionResult> Index()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            var listToReturn = await _context.Simulations
                .Include(s => s.User)
                .Include(s => s.Memories)
                .AsNoTracking()
                .OrderBy(s => s.CreateDate)
                .ToListAsync();

            return View(listToReturn);
        }

        // GET: Simulations/Details/5
        [RedirectAction]
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id == null)
            {
                return RedirectToAction("Error404", "Erros");
            }

            var simulation = await _context.Simulations
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (simulation == null)
            {
                return RedirectToAction("Error404", "Erros");
            }

            return View(simulation);
        }

        // GET: Simulations/Create
        [RedirectAction]
        public IActionResult Create()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            return View();
        }

        // POST: Simulations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> Create([Bind("ID,Name")] Simulation simulation)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            simulation.CreateDate = DateTime.Now;
            simulation.UserID = HttpContext.Session.GetInt32("UserID");

            if (ModelState.IsValid)
            {
                _context.Add(simulation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(simulation);
        }

        // GET: Simulations/Edit/5
        [RedirectAction]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id == null)
            {
                return RedirectToAction("Error404", "Erros");
            }

            var simulation = await _context.Simulations.FindAsync(id);
            if (simulation == null)
            {
                return RedirectToAction("Error404", "Erros");
            }
            return View(simulation);
        }

        // POST: Simulations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,CreateDate,UserID")] Simulation simulation)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id != simulation.ID)
            {
                return RedirectToAction("Error404", "Erros");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(simulation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SimulationExists(simulation.ID))
                    {
                        return RedirectToAction("Error404", "Erros");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(simulation);
        }

        // GET: Simulations/Delete/5
        [RedirectAction]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id == null)
            {
                return RedirectToAction("Error404", "Erros");
            }

            var simulation = await _context.Simulations
                .FirstOrDefaultAsync(m => m.ID == id);
            if (simulation == null)
            {
                return RedirectToAction("Error404", "Erros");
            }

            return View(simulation);
        }

        // POST: Simulations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            var simulation = await _context.Simulations.FindAsync(id);
            _context.Simulations.Remove(simulation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [RedirectAction]
        private bool SimulationExists(int id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            return _context.Simulations.Any(e => e.ID == id);
        }

        /*public void InsertProcess(int index, Process process)
        {
            var framesNeeded = process.RegL / this.FramesSize;
            framesNeeded = process.RegL % this.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;
            Frame frame;

            try
            {
                if (this.Frames[index].Process != null)
                    throw new Exception("Não é possível inserir o processo, pois esse local da mémoria já está sendo utilizado!");
                else
                {
                    if (framesNeeded == 1)
                    {
                        frame = this.Frames[index];
                        process.RegB = frame.RegB;
                        if (frame.Process == null)
                            frame.Process = process;
                        else
                            throw new Exception("Não é possível inserir o processo, pois esse local da mémoria já está sendo utilizado!");
                    }
                    else
                    {
                        for (int i = 0; i < framesNeeded; i++)
                        {
                            if (this.Frames[index + i].Process != null)
                                throw new Exception("Não é possível inserir o processo, pois esse local da mémoria já está sendo utilizado!");
                            else
                            {
                                frame = this.Frames[index + i];
                                process.RegB = frame.RegB;
                                frame.Process = process;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int FirstFitInsertion(Process pProcess)
        {
            var framesNeeded = pProcess.RegL / this.FramesSize;
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
            }
            return -1;
        }

        public int BestFitInsertion(Process pProcess)
        {
            var framesNeeded = pProcess.RegL / this.FramesSize;
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
            return bestFit.Key;
        }

        public int WorstFitInsertion(Process pProcess)
        {
            var framesNeeded = pProcess.RegL / this.FramesSize;
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
            return worstFit.Key;
        }

        public int QuickFitInsertion(Process pProcess, List<KeyValuePair<long, long>> mappedMemory, Dictionary<long, bool> allRegB)
        {
            var framesNeeded = pProcess.RegL / this.FramesSize;
            framesNeeded = pProcess.RegL % this.FramesSize > 0 ? framesNeeded + 1 : framesNeeded;

            if (!mappedMemory.Any(a => a.Value >= framesNeeded))
                return -1;

            var placeToInsert = mappedMemory.Where(w => w.Value >= framesNeeded).FirstOrDefault().Key;
            var auxRegb = placeToInsert;

            for (long usedFrames = 1; usedFrames <= framesNeeded; usedFrames++, auxRegb += FramesSize)
                allRegB[auxRegb] = true;

            pProcess.TimeToFindIndex = 0;
            return Convert.ToInt32(placeToInsert);
        }

        public void PrintMemory()
        {
            List<Frame> listToPrint = new List<Frame>();
            listToPrint.AddRange(this.Frames);
            //listToPrint.Reverse();

            Console.WriteLine("--------------MEMORY-------------------\n");
            foreach (var frame in listToPrint)
            {
                if (frame.Process != null)
                    Console.WriteLine(String.Format("{1} - {2} => {0}", frame.Process.Name, frame.RegB, frame.RegB + this.FramesSize));
                else
                    Console.WriteLine(String.Format("{1} - {2} => {0}", "---", frame.RegB, frame.RegB + this.FramesSize));
            }
            Console.WriteLine("\n---------------------------------------\n\n\n");
        }*/
    }
}
