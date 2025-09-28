using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            var viewModel = new DispositivoCreateViewModel();
            return View(viewModel);
        }

        // POST: Dispositivo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DispositivoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dispositivo = new Dispositivo
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    IdMarca = model.IdMarca,
                    IdTipo = model.IdTipo,
                    CodigoInventario = model.CodigoInventario,
                    NroSerie = model.NroSerie,
                    Estado = model.Estado ?? "Activo",
                    FechaAlta = DateOnly.FromDateTime(DateTime.Now),
                    EstadoRegistro = true,
                    StockMinimo = 0 // Por defecto
                };

                // Obtener el tipo para saber si es hardware o consumible
                var tipo = await _context.TipoHardwares.FindAsync(model.IdTipo);
                if (tipo?.Descripcion == "Consumible")
                {
                    dispositivo.StockActual = model.CantidadInicial ?? 0;
                }
                else
                {
                    dispositivo.StockActual = 1; // Por defecto para hardware
                }

                _context.Add(dispositivo);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);

            return View(model);
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
            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", dispositivo.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", dispositivo.IdTipo);
            return View(dispositivo);
        }

        // POST: Dispositivo/Edit/5
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
            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", dispositivo.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", dispositivo.IdTipo);
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