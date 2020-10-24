using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;

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
            return View(await _context.Memories.ToListAsync());
        }

        // GET: Memories/Details/5
        [RedirectAction]
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id == null)
            {
                return NotFound();
            }

            var memory = await _context.Memories
                .FirstOrDefaultAsync(m => m.ID == id);
            if (memory == null)
            {
                return NotFound();
            }

            return View(memory);
        }

        // GET: Memories/Create
        [RedirectAction]
        public IActionResult Create()
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            return View();
        }

        // POST: Memories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> Create([Bind("ID,Name,CreateDate")] Memory memory)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (ModelState.IsValid)
            {
                _context.Add(memory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(memory);
        }

        // GET: Memories/Edit/5
        [RedirectAction]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id == null)
            {
                return NotFound();
            }

            var memory = await _context.Memories.FindAsync(id);
            if (memory == null)
            {
                return NotFound();
            }
            return View(memory);
        }

        // POST: Memories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,CreateDate")] Memory memory)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id != memory.ID)
            {
                return NotFound();
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
            return View(memory);
        }

        // GET: Memories/Delete/5
        [RedirectAction]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            if (id == null)
            {
                return NotFound();
            }

            var memory = await _context.Memories
                .FirstOrDefaultAsync(m => m.ID == id);
            if (memory == null)
            {
                return NotFound();
            }

            return View(memory);
        }

        // POST: Memories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");

            var memory = await _context.Memories.FindAsync(id);
            _context.Memories.Remove(memory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [RedirectAction]
        private bool MemoryExists(int id)
        {
            return _context.Memories.Any(e => e.ID == id);
        }
    }
}
