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

        // GET: Dispositivo/Details/5 - Para modal
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

            return PartialView("_DetailsPartial", dispositivo);
        }

        // GET: Dispositivo/Create - Para modal
        public IActionResult Create()
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre");
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion");
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            var viewModel = new DispositivoCreateViewModel
            {
                IdTipo = 1 // Preseleccionar "Hardware" (ID = 1)
            };

            return PartialView("_CreatePartial", viewModel);
        }

        // POST: Dispositivo/CreateModal - Nuevo método para AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(DispositivoCreateViewModel model)
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
                    Estado = model.Estado ?? "Nuevo",
                    FechaAlta = DateOnly.FromDateTime(DateTime.Now),
                    EstadoRegistro = true,
                    StockMinimo = 0
                };

                // Si es consumible (IdTipo == 2), usar la cantidad inicial
                if (model.IdTipo == 2)
                {
                    dispositivo.StockActual = model.CantidadInicial ?? 0;
                }
                else
                {
                    dispositivo.StockActual = 1; // Por defecto para hardware
                }

                _context.Add(dispositivo);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_CreatePartial", model);
        }

        // GET: Dispositivo/Edit/5 - Para modal
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

            var viewModel = new DispositivoEditViewModel
            {
                IdDispositivo = dispositivo.IdDispositivo,
                Nombre = dispositivo.Nombre,
                Descripcion = dispositivo.Descripcion,
                IdMarca = dispositivo.IdMarca,
                IdTipo = dispositivo.IdTipo,
                CodigoInventario = dispositivo.CodigoInventario,
                NroSerie = dispositivo.NroSerie,
                Estado = dispositivo.Estado,
                FechaAlta = dispositivo.FechaAlta,
                FechaBaja = dispositivo.FechaBaja,
                StockActual = dispositivo.StockActual,
                StockMinimo = dispositivo.StockMinimo,
                CantidadInicial = dispositivo.StockActual
            };

            var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
            var tipos = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();

            if (!marcas.Any() || !tipos.Any())
            {
                TempData["ErrorMessage"] = "No hay marcas o tipos disponibles. Por favor, agregue al menos uno.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", viewModel.IdMarca);
            ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", viewModel.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_EditPartial", viewModel);
        }

        // POST: Dispositivo/EditModal - Nuevo método para AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, DispositivoEditViewModel model)
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

                    dispositivo.Nombre = model.Nombre ?? "Sin nombre";
                    dispositivo.Descripcion = model.Descripcion;
                    dispositivo.IdMarca = model.IdMarca ?? 0;
                    dispositivo.IdTipo = model.IdTipo ?? 0;
                    dispositivo.CodigoInventario = model.CodigoInventario;
                    dispositivo.NroSerie = model.NroSerie;
                    dispositivo.Estado = model.Estado;
                    dispositivo.FechaBaja = model.FechaBaja;
                    dispositivo.StockMinimo = model.StockMinimo ?? 0;

                    // Solo actualizar StockActual si es consumible
                    var tipo = await _context.TipoHardwares.FindAsync(model.IdTipo);
                    if (tipo?.Descripcion?.Trim().ToLower() == "consumible")
                    {
                        dispositivo.StockActual = model.CantidadInicial ?? dispositivo.StockActual;
                    }

                    _context.Update(dispositivo);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
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
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_EditPartial", model);
        }

        // POST: Dispositivo/Delete/5 - Para modal con AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo != null)
            {
                dispositivo.EstadoRegistro = false; // Borrado lógico
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // Método privado para verificar si existe un dispositivo
        private bool DispositivoExists(int id)
        {
            return _context.Dispositivos.Any(e => e.IdDispositivo == id);
        }
    }
}