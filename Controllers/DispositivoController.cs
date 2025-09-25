using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using inventario_coprotab.Models.DBInventario;

namespace inventario_coprotab.Controllers
{
    public class DispositivoController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public DispositivoController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // GET: Dispositivo
        public async Task<IActionResult> Index(string searchCode, string searchSerie, string searchTipo, string searchEstado)
        {
            var query = _context.Dispositivos
                .Include(d => d.IdMarcaNavigation)
                .Include(d => d.IdTipoNavigation)
                .Where(d => d.EstadoRegistro); // Solo activos

            if (!string.IsNullOrEmpty(searchCode))
                query = query.Where(d => d.CodigoInventario != null && d.CodigoInventario.Contains(searchCode));

            if (!string.IsNullOrEmpty(searchSerie))
                query = query.Where(d => d.NroSerie != null && d.NroSerie.Contains(searchSerie));

            if (!string.IsNullOrEmpty(searchTipo))
                query = query.Where(d => d.IdTipoNavigation != null && d.IdTipoNavigation.Descripcion.Contains(searchTipo));

            if (!string.IsNullOrEmpty(searchEstado))
                query = query.Where(d => d.Estado == searchEstado);

            var dispositivos = await query.ToListAsync();

            ViewBag.SearchCode = searchCode;
            ViewBag.SearchSerie = searchSerie;
            ViewBag.SearchTipo = searchTipo;
            ViewBag.SearchEstado = searchEstado;

            return View(dispositivos);
        }

        // GET: Dispositivo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivo = await _context.Dispositivos
                .Include(d => d.IdMarcaNavigation)
                .Include(d => d.IdTipoNavigation)
                .FirstOrDefaultAsync(m => m.IdDispositivo == id);
            if (dispositivo == null)
            {
                return NotFound();
            }

            return View(dispositivo);
        }

        // GET: Dispositivo/Create
        public IActionResult Create()
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre");
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion");
            return View();
        }

        // POST: Dispositivo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Dispositivo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDispositivo,Nombre,Descripcion,IdMarca,IdTipo,CodigoInventario,NroSerie,Estado,FechaAlta,FechaBaja,StockActual,StockMinimo,EstadoRegistro")] Dispositivo dispositivo)
        {
            if (ModelState.IsValid)
            {
                dispositivo.FechaAlta = DateTime.Now.Date; // ⚡ Auto-llenar fecha alta
                dispositivo.EstadoRegistro = true;         // Por defecto, activo
                _context.Add(dispositivo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", dispositivo.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", dispositivo.IdTipo);
            return View(dispositivo);
        }

        // GET: Dispositivo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo == null)
            {
                return NotFound();
            }
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "IdMarca", "IdMarca", dispositivo.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares, "IdTipo", "IdTipo", dispositivo.IdTipo);
            return View(dispositivo);
        }

        // POST: Dispositivo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDispositivo,Nombre,Descripcion,IdMarca,IdTipo,CodigoInventario,NroSerie,Estado,FechaAlta,FechaBaja,StockActual,StockMinimo,EstadoRegistro")] Dispositivo dispositivo)
        {
            if (id != dispositivo.IdDispositivo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dispositivo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DispositivoExists(dispositivo.IdDispositivo))
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
            ViewData["IdMarca"] = new SelectList(_context.Marcas, "IdMarca", "IdMarca", dispositivo.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares, "IdTipo", "IdTipo", dispositivo.IdTipo);
            return View(dispositivo);
        }

        // GET: Dispositivo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dispositivo = await _context.Dispositivos
                .Include(d => d.IdMarcaNavigation)
                .Include(d => d.IdTipoNavigation)
                .FirstOrDefaultAsync(m => m.IdDispositivo == id);
            if (dispositivo == null)
            {
                return NotFound();
            }

            return View(dispositivo);
        }

        // POST: Dispositivo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo != null)
            {
                dispositivo.EstadoRegistro = false; // Borrado lógico
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DispositivoExists(int id)
        {
            return _context.Dispositivos.Any(e => e.IdDispositivo == id);
        }
    }
}
