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

            var viewModel = new DispositivoCreateViewModel
            {
                IdTipo = 1 // Preseleccionar "Hardware" (ID = 1)
            };
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

                // ✅ Mejor: usar el ID directamente (más seguro)
                if (model.IdTipo == 2) // Cambia 2 por el ID real de "Consumible"
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

            // Convertir Dispositivo a ViewModel
            var viewModel = new DispositivoEditViewModel
            {
                IdDispositivo = dispositivo.IdDispositivo,
                Nombre = dispositivo.Nombre,
                Descripcion = dispositivo.Descripcion,
                IdMarca = dispositivo.IdMarca, // ✅ Ya es int? en el ViewModel
                IdTipo = dispositivo.IdTipo,   // ✅ Ya es int? en el ViewModel
                CodigoInventario = dispositivo.CodigoInventario,
                NroSerie = dispositivo.NroSerie,
                Estado = dispositivo.Estado,
                FechaAlta = dispositivo.FechaAlta,
                FechaBaja = dispositivo.FechaBaja,
                StockActual = dispositivo.StockActual, // ✅ Ya es int? en el ViewModel
                StockMinimo = dispositivo.StockMinimo, // ✅ Ya es int? en el ViewModel
                EstadoRegistro = dispositivo.EstadoRegistro,
                CantidadInicial = dispositivo.StockActual // Puedes usar este campo para editar stock si es consumible
            };

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", viewModel.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", viewModel.IdTipo);

            return View(viewModel);
        }

        // POST: Dispositivo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DispositivoEditViewModel model)
        {
            if (id != model.IdDispositivo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dispositivo = await _context.Dispositivos.FindAsync(id);
                    if (dispositivo == null)
                    {
                        return NotFound();
                    }

                    // Mapear ViewModel a Entidad
                    // ✅ CS8601 resuelto
                    dispositivo.Nombre = model.Nombre ?? "Sin nombre";
                    dispositivo.Descripcion = model.Descripcion;
                    dispositivo.IdMarca = model.IdMarca ?? 0; // ✅ Usa ?? para proporcionar un valor predeterminado
                    dispositivo.IdTipo = model.IdTipo ?? 0;   // ✅ Usa ?? para proporcionar un valor predeterminado
                    dispositivo.CodigoInventario = model.CodigoInventario;
                    dispositivo.NroSerie = model.NroSerie;
                    dispositivo.Estado = model.Estado;
                    dispositivo.FechaBaja = model.FechaBaja;
                    dispositivo.StockMinimo = model.StockMinimo ?? 0; // ✅ Usa ?? para proporcionar un valor predeterminado
                    dispositivo.EstadoRegistro = model.EstadoRegistro;

                    // Solo actualizar StockActual si es consumible
                    var tipo = await _context.TipoHardwares.FindAsync(model.IdTipo);
                    if (tipo?.Descripcion?.Trim().ToLower() == "consumible")
                    {
                        dispositivo.StockActual = model.CantidadInicial ?? dispositivo.StockActual;
                    }

                    _context.Update(dispositivo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DispositivoExists(model.IdDispositivo))
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

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);

            return View(model);
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