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
    }
}
