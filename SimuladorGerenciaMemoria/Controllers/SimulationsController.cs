using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimuladorGerenciaMemoria;
using SimuladorGerenciaMemoria.Models;

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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Simulations.ToListAsync());
        }

        // GET: Simulations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var simulation = await _context.Simulations
                .FirstOrDefaultAsync(m => m.ID == id);
            if (simulation == null)
            {
                return NotFound();
            }

            return View(simulation);
        }

        // GET: Simulations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Simulations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,CreateDate")] Simulation simulation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(simulation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(simulation);
        }

        // GET: Simulations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var simulation = await _context.Simulations.FindAsync(id);
            if (simulation == null)
            {
                return NotFound();
            }
            return View(simulation);
        }

        // POST: Simulations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,CreateDate")] Simulation simulation)
        {
            if (id != simulation.ID)
            {
                return NotFound();
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
                        return NotFound();
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var simulation = await _context.Simulations
                .FirstOrDefaultAsync(m => m.ID == id);
            if (simulation == null)
            {
                return NotFound();
            }

            return View(simulation);
        }

        // POST: Simulations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var simulation = await _context.Simulations.FindAsync(id);
            _context.Simulations.Remove(simulation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SimulationExists(int id)
        {
            return _context.Simulations.Any(e => e.ID == id);
        }
    }
}
