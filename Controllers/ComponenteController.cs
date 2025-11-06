using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        // ========================================
        // Agregar este using al inicio del archivo ComponenteController.cs
        // ========================================
        // using Microsoft.EntityFrameworkCore;

        // POST: Componente/CreateModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(ComponenteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Validar si el número de serie ya existe (si se proporciona)
                    if (!string.IsNullOrWhiteSpace(model.NroSerie))
                    {
                        var existeSerie = await _context.Componentes
                            .AnyAsync(c => c.NroSerie == model.NroSerie && c.EstadoRegistro);

                        if (existeSerie)
                        {
                            ModelState.AddModelError("NroSerie", "Ya existe un componente con este número de serie.");

                            // Recargar datos para la vista
                            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
                            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares
                                .Where(t => t.Descripcion != "Hardware")
                                .Where(t => t.Descripcion != "Equipo Armado")
                                .OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
                            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                            return PartialView("_CreatePartial", model);
                        }
                    }



                    var componente = new  Models.DBInventario.Componente
                    {
                        Nombre = model.Nombre,
                        Descripcion = model.Descripcion,
                        IdMarca = model.IdMarca,
                        IdTipo = model.IdTipo,
                        NroSerie = string.IsNullOrWhiteSpace(model.NroSerie) ? null : model.NroSerie.Trim(),
                        Estado = model.Estado ?? "Nuevo",
                        FechaInstalacion = model.FechaInstalacion,
                        EstadoRegistro = true,
                        Cantidad = model.Cantidad ?? 1,
                        StockMinimo = model.StockMinimo ?? 0
                    };



                    _context.Componentes.Add(componente);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }
                catch (DbUpdateException ex)
                {
                    // ✅ Capturar errores de base de datos
                    var errorMessage = "Error al guardar el componente.";

                    if (ex.InnerException != null)
                    {
                        var innerMessage = ex.InnerException.Message;

                        if (innerMessage.Contains("UQ__componen__AD64A161") || innerMessage.Contains("nro_serie"))
                        {
                            errorMessage = "El número de serie ya existe en la base de datos.";
                            ModelState.AddModelError("NroSerie", errorMessage);
                        }
                        else if (innerMessage.Contains("UNIQUE"))
                        {
                            errorMessage = "Ya existe un registro con estos datos únicos.";
                            ModelState.AddModelError("", errorMessage);
                        }
                        else
                        {
                            errorMessage = "Error de base de datos: " + innerMessage;
                            ModelState.AddModelError("", errorMessage);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", errorMessage);
                    }

                    // Recargar datos para la vista
                    ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
                    ViewData["IdTipo"] = new SelectList(_context.TipoHardwares
                        .Where(t => t.Descripcion != "Hardware")
                        .OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
                    ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                    return PartialView("_CreatePartial", model);
                }
                catch (Exception ex)
                {
                    // ✅ Capturar cualquier otro error
                    ModelState.AddModelError("", "Error inesperado: " + ex.Message);

                    // Recargar datos para la vista
                    ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
                    ViewData["IdTipo"] = new SelectList(_context.TipoHardwares
                        .Where(t => t.Descripcion != "Hardware")
                        .OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
                    ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                    return PartialView("_CreatePartial", model);
                }
            }

            // ✅ Si hay errores de validación, recargar datos y devolver la vista parcial
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

        // POST: Componente/Edit
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

            try
            {
                // ✅ Validar si el número de serie ya existe en otro componente
                if (!string.IsNullOrWhiteSpace(model.NroSerie))
                {
                    var existeSerie = await _context.Componentes
                        .AnyAsync(c => c.NroSerie == model.NroSerie &&
                                       c.IdComponente != model.IdComponente &&
                                       c.EstadoRegistro);

                    if (existeSerie)
                    {
                        ModelState.AddModelError("NroSerie", "Ya existe otro componente con este número de serie.");

                        var marcasError = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                        var tiposError = await _context.TipoHardwares
                            .Where(t => t.Descripcion != "Hardware")
                            .OrderBy(t => t.Descripcion)
                            .ToListAsync();

                        ViewData["IdMarca"] = new SelectList(marcasError, "IdMarca", "Nombre", model.IdMarca);
                        ViewData["IdTipo"] = new SelectList(tiposError, "IdTipo", "Descripcion", model.IdTipo);
                        ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                        return PartialView("_EditPartial", model);
                    }
                }

                // Actualizar campos
                componente.Nombre = model.Nombre;
                componente.Descripcion = model.Descripcion;
                componente.IdMarca = model.IdMarca;
                componente.IdTipo = model.IdTipo;
                componente.NroSerie = string.IsNullOrWhiteSpace(model.NroSerie) ? null : model.NroSerie.Trim();
                componente.Estado = model.Estado;
                componente.FechaInstalacion = model.FechaInstalacion;
                componente.Cantidad = model.Cantidad;
                componente.StockMinimo = model.StockMinimo ?? 1;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = "Error al actualizar el componente.";

                if (ex.InnerException != null)
                {
                    var innerMessage = ex.InnerException.Message;

                    if (innerMessage.Contains("UQ__componen__AD64A161") || innerMessage.Contains("nro_serie"))
                    {
                        errorMessage = "El número de serie ya existe en la base de datos.";
                        ModelState.AddModelError("NroSerie", errorMessage);
                    }
                    else if (innerMessage.Contains("UNIQUE"))
                    {
                        errorMessage = "Ya existe un registro con estos datos únicos.";
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        errorMessage = "Error de base de datos: " + innerMessage;
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError("", errorMessage);
                }

                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares
                    .Where(t => t.Descripcion != "Hardware")
                    .OrderBy(t => t.Descripcion)
                    .ToListAsync();

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                return PartialView("_EditPartial", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error inesperado: " + ex.Message);

                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares
                    .Where(t => t.Descripcion != "Hardware")
                    .OrderBy(t => t.Descripcion)
                    .ToListAsync();

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

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