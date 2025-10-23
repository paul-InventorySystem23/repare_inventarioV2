using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace inventario_coprotab.Controllers
{
    public class ComponenteController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public ComponenteController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // GET: Componente/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var componente = await _context.Componentes
                .Include(c => c.IdMarcaNavigation)
                .Include(c => c.IdTipoNavigation)
                .FirstOrDefaultAsync(m => m.IdComponente == id);

            if (componente == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", componente);
        }

        // GET: Componente/Create
        public IActionResult Create()
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre");
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion");
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            var viewModel = new ComponenteCreateViewModel();
            return PartialView("_CreatePartial", viewModel);
        }

        // POST: Componente/CreateModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(ComponenteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var componente = new Componente
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    IdMarca = model.IdMarca,
                    IdTipo = model.IdTipo,
                    NroSerie = model.NroSerie,
                    Estado = model.Estado ?? "Nuevo",
                    FechaInstalacion = model.FechaInstalacion,
                    EstadoRegistro = true,
                    Cantidad = model.Cantidad ?? 1,
                    StockMinimo = model.StockMinimo ?? 0
                };

                _context.Add(componente);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Dispositivo");
                //return Json(new { success = true });
            }

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares
                .Where(t => t.Descripcion != "Hardware")
                .OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_CreatePartial", model);
        }

        // GET: Componente/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var componente = await _context.Componentes.FindAsync(id);
            if (componente == null)
            {
                return NotFound();
            }

            var model = new ComponenteEditViewModel
            {
                IdComponente = componente.IdComponente,
                Nombre = componente.Nombre,
                Descripcion = componente.Descripcion,
                IdMarca = componente.IdMarca,
                IdTipo = componente.IdTipo,
                NroSerie = componente.NroSerie,
                Estado = componente.Estado,
                FechaInstalacion = componente.FechaInstalacion,
                Cantidad = componente.Cantidad,
                StockMinimo = componente.StockMinimo
            };

            var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
            var tipos = await _context.TipoHardwares
                .Where(t => t.Descripcion != "Hardware")
                .OrderBy(t => t.Descripcion)
                .ToListAsync();

            if (!marcas.Any() || !tipos.Any())
            {
                TempData["ErrorMessage"] = "No hay marcas o tipos disponibles.";
                return RedirectToAction("Index", "Dispositivo");
            }

            ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_EditPartial", model);
        }

        // POST: Componente/EditModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ComponenteEditViewModel model)
        {
            if (model.IdComponente <= 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = new SelectList(_context.TipoHardwares
                .Where(t => t.Descripcion != "Hardware")
                .OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                return PartialView("_EditPartial", model);
            }

            var componente = await _context.Componentes.FindAsync(model.IdComponente);
            if (componente == null)
            {
                return NotFound();
            }

            // Actualizar campos
            componente.Nombre = model.Nombre;
            componente.Descripcion = model.Descripcion;
            componente.IdMarca = model.IdMarca;
            componente.IdTipo = model.IdTipo;
            componente.NroSerie = model.NroSerie;
            componente.Estado = model.Estado;
            componente.FechaInstalacion = model.FechaInstalacion;
            componente.Cantidad = model.Cantidad;
            componente.StockMinimo = model.StockMinimo ?? 1;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares
                    .Where(t => t.Descripcion != "Hardware" && t.Descripcion != "Consumible")
                    .OrderBy(t => t.Descripcion)
                    .ToListAsync();

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                ModelState.AddModelError("", "Ocurrió un error al guardar los cambios.");
                return PartialView("_EditPartial", model);
            }
        }

        // POST: Componente/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var componente = await _context.Componentes.FindAsync(id);
            if (componente != null)
            {
                componente.EstadoRegistro = false;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}