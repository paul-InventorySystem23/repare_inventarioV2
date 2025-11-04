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

        // ✅ Método auxiliar para generar código de inventario automático
        private async Task<string> GenerarCodigoInventario()
        {
            var año = DateTime.Now.Year;

            // Obtener el último código del año actual
            var ultimoCodigo = await _context.Dispositivos
                .Where(d => d.CodigoInventario != null && d.CodigoInventario.StartsWith($"INV-{año}-"))
                .OrderByDescending(d => d.CodigoInventario)
                .Select(d => d.CodigoInventario)
                .FirstOrDefaultAsync();

            int numeroSecuencial = 1;

            if (!string.IsNullOrEmpty(ultimoCodigo))
            {
                // Extraer el número secuencial del último código
                var partes = ultimoCodigo.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int numero))
                {
                    numeroSecuencial = numero + 1;
                }
            }

            // Formato: INV-2025-0001
            return $"INV-{año}-{numeroSecuencial:D4}";
        }

        // GET: Dispositivo
        public async Task<IActionResult> Index(string searchNombre, string searchSerie, string searchTipo, string searchEstado, bool? mostrarAlertas)
        {
            // DISPOSITIVOS (Hardware y Consumible)
            var queryDispositivos = _context.Dispositivos
                .Include(d => d.IdMarcaNavigation)
                .Include(d => d.IdTipoNavigation)
                .Where(d => d.EstadoRegistro);

            if (!string.IsNullOrEmpty(searchNombre))
                queryDispositivos = queryDispositivos
                .Where(d => d.Nombre != null &&
                EF.Functions.Like(d.Nombre.ToLower(), $"%{searchNombre.ToLower()}%"));

            if (!string.IsNullOrEmpty(searchSerie))
                queryDispositivos = queryDispositivos.Where(d => d.NroSerie != null && d.NroSerie.Contains(searchSerie));

            if (!string.IsNullOrEmpty(searchTipo))
                queryDispositivos = queryDispositivos.Where(d => d.IdTipoNavigation != null && d.IdTipoNavigation.Descripcion.Contains(searchTipo));

            if (!string.IsNullOrEmpty(searchEstado))
                queryDispositivos = queryDispositivos.Where(d => d.Estado == searchEstado);

            // ✅ NUEVO: Filtrar por alertas de bajo stock
            if (mostrarAlertas == true)
            {
                queryDispositivos = queryDispositivos.Where(d => d.StockActual <= d.StockMinimo);
                            }

            // ✅ Ordenar por fecha de alta descendente (más recientes primero)
            var dispositivos = await queryDispositivos
                .OrderByDescending(d => d.FechaAlta)
                //.Take(mostrarAlertas == true ? int.MaxValue : 5)
                .ToListAsync();

            // COMPONENTES
            var queryComponentes = _context.Componentes
                .Include(c => c.IdMarcaNavigation)
                .Include(c => c.IdTipoNavigation)
                .Where(c => c.EstadoRegistro);

            // Aplicar filtros similares para componentes
            if (!string.IsNullOrEmpty(searchNombre))
                queryComponentes = queryComponentes
                .Where(c => c.Nombre != null &&
                EF.Functions.Like(c.Nombre.ToLower(), $"%{searchNombre.ToLower()}%"));

            if (!string.IsNullOrEmpty(searchSerie))
                queryComponentes = queryComponentes.Where(c => c.NroSerie != null && c.NroSerie.Contains(searchSerie));

            if (!string.IsNullOrEmpty(searchTipo))
                queryComponentes = queryComponentes.Where(c => c.IdTipoNavigation != null && c.IdTipoNavigation.Descripcion.Contains(searchTipo));

            if (!string.IsNullOrEmpty(searchEstado))
                queryComponentes = queryComponentes.Where(c => c.Estado == searchEstado);

            // ✅ NUEVO: Filtrar por alertas de bajo stock en componentes
            if (mostrarAlertas == true)
            {
                queryComponentes = queryComponentes.Where(c => c.Cantidad <= c.StockMinimo);
            }

            // ✅ Ordenar por fecha de instalación descendente
            var componentes = await queryComponentes
                .OrderByDescending(c => c.FechaInstalacion)
                //.Take(mostrarAlertas == true ? int.MaxValue : 5)
                .ToListAsync();

            // Movimientos de DISPOSITIVOS
            var queryMovimientos = _context.Movimientos
                .Include(c => c.IdDispositivoNavigation)
                .Include(c => c.IdUbicacionNavigation)
                .Include(c => c.IdResponsableNavigation)
                .Where(m => m.IdDispositivo != null);

            var movimientos = await queryMovimientos
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            // Movimientos de COMPONENTES
            var queryMovimientosComponentes = _context.Movimientos
                .Include(c => c.IdComponenteNavigation)
                .Include(c => c.IdUbicacionNavigation)
                .Include(c => c.IdResponsableNavigation)
                .Where(m => m.IdComponente != null);

            var movimientosComponentes = await queryMovimientosComponentes
                .OrderByDescending(m => m.Fecha)
                //.Take(5)
                .ToListAsync();

            // Pasar todas las listas a la vista
            ViewBag.Movimientos = movimientos;
            ViewBag.MovimientosComponentes = movimientosComponentes;
            ViewBag.Componentes = componentes;
            ViewBag.searchNombre = searchNombre;
            ViewBag.SearchSerie = searchSerie;
            ViewBag.SearchTipo = searchTipo;
            ViewBag.SearchEstado = searchEstado;
            ViewBag.MostrarAlertas = mostrarAlertas; // ✅ Agregar esta línea

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

            return PartialView("_DetailsPartial", dispositivo);
        }

        // GET: Dispositivo/Create
        //public IActionResult Create()
        //{
        //    ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre");
        //    ViewData["IdTipo"] = new SelectList(_context.TipoHardwares
        //        .Where(t => t.Descripcion == "Hardware")
        //        .OrderBy(t => t.Descripcion), "IdTipo", "Descripcion");            
        //    ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

        //    var viewModel = new DispositivoCreateViewModel
        //    {
        //        IdTipo = 1
        //    };

        //    return PartialView("_CreatePartial", viewModel);
        //}



        public IActionResult Create()
        {
            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre");
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares
         .Where(t => t.Descripcion == "Hardware")
         .OrderBy(t => t.Descripcion), "IdTipo", "Descripcion");
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            var viewModel = new DispositivoCreateViewModel();
            return PartialView("_CreatePartial", viewModel);
        }

// ==================== MÉTODO CREATE CORREGIDO ====================
// POST: Dispositivo/CreateModal
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateModal(DispositivoCreateViewModel model)
{
    if (ModelState.IsValid)
    {
        try 
        { 
            // ✅ Validar si el número de serie ya existe (si se proporciona)
            if (!string.IsNullOrWhiteSpace(model.NroSerie))
            {
                var existeSerie = await _context.Dispositivos
                    .AnyAsync(d => d.NroSerie == model.NroSerie && d.EstadoRegistro);

                if (existeSerie)
                {
                    ModelState.AddModelError("NroSerie", "Ya existe un Dispositivo con esta identificación única.");

                    // Recargar datos para la vista
                    ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
                    ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
                    ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

                    return PartialView("_CreatePartial", model);
                }
            }

            // ✅ Crear el dispositivo
            // ✅ Generar código automáticamente
            var codigoInventario = await GenerarCodigoInventario();
            var dispositivo = new Dispositivo
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                IdMarca = model.IdMarca,
                IdTipo = model.IdTipo,
                CodigoInventario = codigoInventario, // ✅ Código automático
                NroSerie = string.IsNullOrWhiteSpace(model.NroSerie) ? null : model.NroSerie?.Trim(),
                Estado = model.Estado ?? "Nuevo",
                FechaAlta = DateTime.Now,
                EstadoRegistro = true,
                StockMinimo = 0
            };

            // ✅ Asignar StockActual según el tipo
            if (model.IdTipo == 2) // Si es Consumible
            {
                dispositivo.StockActual = model.CantidadInicial ?? 0;
            }
            else
            {
                dispositivo.StockActual = 1;
            }

            _context.Add(dispositivo);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (DbUpdateException ex)
        {
            var errorMessage = "Error al guardar el dispositivo.";

            if (ex.InnerException != null)
            {
                var innerMessage = ex.InnerException.Message;

                if (innerMessage.Contains("UQ__disposit__AD64A1611432F6A2") || innerMessage.Contains("nro_serie"))
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

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_CreatePartial", model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error inesperado: " + ex.Message);

            ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
            ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

            return PartialView("_CreatePartial", model);
        }
    }

    // ✅ Si hay errores de validación, recargar datos y devolver la vista parcial
    ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
    ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
    ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

    return PartialView("_CreatePartial", model);
}

//        // ✅ Generar código automáticamente
//        var codigoInventario = await GenerarCodigoInventario();

//        var dispositivo = new Dispositivo
//        {
//            Nombre = model.Nombre,
//            Descripcion = model.Descripcion,
//            IdMarca = model.IdMarca,
//            IdTipo = model.IdTipo,
//            CodigoInventario = codigoInventario, // ✅ Código automático
//            NroSerie = model.NroSerie,
//            Estado = model.Estado ?? "Nuevo",
//            FechaAlta = DateTime.Now, // ✅ DateTime con hora
//            EstadoRegistro = true,
//            StockMinimo = 0
//        };

//        if (model.IdTipo == 2)
//        {
//            dispositivo.StockActual = model.CantidadInicial ?? 0;
//        }
//        else
//        {
//            dispositivo.StockActual = 1;
//        }

//        _context.Add(dispositivo);
//        await _context.SaveChangesAsync();

//        return Json(new { success = true });
//    }

//    ViewData["IdMarca"] = new SelectList(_context.Marcas.OrderBy(m => m.Nombre), "IdMarca", "Nombre", model.IdMarca);
//    ViewData["IdTipo"] = new SelectList(_context.TipoHardwares.OrderBy(t => t.Descripcion), "IdTipo", "Descripcion", model.IdTipo);
//    ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };

//    return PartialView("_CreatePartial", model);
//}
// GET: Dispositivo/CreateEquipo
public async Task<IActionResult> CreateEquipo()
        {
            var viewModel = new EquipoCreateViewModel
            {
                Marcas = await _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem { Value = m.IdMarca.ToString(), Text = m.Nombre })
                    .ToListAsync(),

                TiposHardware = await _context.TipoHardwares
                    .Where(t => t.Descripcion == "") // Solo tipo "Equipo Armado"
                    .Select(t => new SelectListItem { Value = t.IdTipo.ToString(), Text = t.Descripcion })
                    .ToListAsync(),

                ComponentesDisponibles = await _context.Componentes
                    .Where(c => c.EstadoRegistro)
                    .Include(c => c.IdMarcaNavigation) // 👈 ¡Incluye la relación!
                    .Select(c => new ComponenteCheckboxItem
                    {
                        IdComponente = c.IdComponente,
                        NombreCompleto = $"{c.Nombre} - {(c.NroSerie != null ? c.NroSerie : "Sin serie")} ({(c.IdMarcaNavigation != null ? c.IdMarcaNavigation.Nombre : "Sin marca")})"
                    })
                    .ToListAsync(),
            };

            // Forzar IdTipo = Hardware (asumiendo que "Hardware" tiene IdTipo = 1)
            var tipoHardware = await _context.TipoHardwares
                .FirstOrDefaultAsync(t => t.Descripcion == "Hardware");
            if (tipoHardware != null)
                viewModel.IdTipo = tipoHardware.IdTipo;

            return PartialView("_CreateEquipoPartial", viewModel);
        }

        // ==================== MÉTODO CREATE EQUIPO CORREGIDO ====================
        // POST: Dispositivo/CreateEquipoModal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEquipoModal(EquipoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar listas si hay error
                model.Marcas = await _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem { Value = m.IdMarca.ToString(), Text = m.Nombre })
                    .ToListAsync();

                model.TiposHardware = await _context.TipoHardwares
                    .Where(t => t.Descripcion == "Hardware")
                    .Select(t => new SelectListItem { Value = t.IdTipo.ToString(), Text = t.Descripcion })
                    .ToListAsync();

                model.ComponentesDisponibles = await _context.Componentes
                    .Where(c => c.EstadoRegistro)
                    .Include(c => c.IdMarcaNavigation)
                    .Select(c => new ComponenteCheckboxItem
                    {
                        IdComponente = c.IdComponente,
                        NombreCompleto = $"{c.Nombre} - {(c.NroSerie != null ? c.NroSerie : "Sin serie")} ({(c.IdMarcaNavigation != null ? c.IdMarcaNavigation.Nombre : "Sin marca")})"
                    })
                    .ToListAsync();

                return PartialView("_CreateEquipoPartial", model);
            }

            try
            {
                // ✅ Validar número de serie si existe
                if (!string.IsNullOrWhiteSpace(model.NroSerie))
                {
                    var existeSerie = await _context.Dispositivos
                        .AnyAsync(d => d.NroSerie == model.NroSerie && d.EstadoRegistro);

                    if (existeSerie)
                    {
                        ModelState.AddModelError("NroSerie", "Ya existe un equipo con este número de serie.");

                        // Recargar listas
                        model.Marcas = await _context.Marcas
                            .OrderBy(m => m.Nombre)
                            .Select(m => new SelectListItem { Value = m.IdMarca.ToString(), Text = m.Nombre })
                            .ToListAsync();

                        //model.TiposHardware = await _context.TipoHardwares
                        //    .Where(t => t.Descripcion == "Hardware")
                        //    .Select(t => new SelectListItem { Value = t.IdTipo.ToString(), Text = t.Descripcion })
                        //    .ToListAsync();

                       
                        return PartialView("_EditPartial", model);
                    }
                }

                // ✅ Crear el dispositivo (equipo)
                var codigoInventario = await GenerarCodigoInventario();

                var equipo = new Dispositivo
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    IdMarca = model.IdMarca,
                    CodigoInventario = codigoInventario,
                    NroSerie = string.IsNullOrWhiteSpace(model.NroSerie) ? null : model.NroSerie?.Trim(),
                    Estado = model.Estado,
                    FechaAlta = DateTime.Now,
                    EstadoRegistro = true,
                    StockActual = 1,
                    StockMinimo = 0
                };

                _context.Dispositivos.Add(equipo);
                await _context.SaveChangesAsync(); // Necesario para obtener IdDispositivo

                // ✅ Asociar componentes seleccionados
                if (model.ComponentesSeleccionados != null && model.ComponentesSeleccionados.Any())
                {
                    foreach (var idComponente in model.ComponentesSeleccionados)
                    {
                        var relacion = new RelacionDispositivoComponente
                        {
                            IdDispositivo = equipo.IdDispositivo,
                            IdComponente = idComponente
                        };
                        _context.RelacionDispositivoComponentes.Add(relacion);
                    }

                    await _context.SaveChangesAsync();
                }

                // ✅ Retornar JSON en lugar de RedirectToAction
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = "Error al guardar el equipo.";

                if (ex.InnerException != null)
                {
                    var innerMessage = ex.InnerException.Message;

                    if (innerMessage.Contains("UQ__disposit__AD64A1611432F6A2") || innerMessage.Contains("nro_serie"))
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

                // Recargar listas
                model.Marcas = await _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem { Value = m.IdMarca.ToString(), Text = m.Nombre })
                    .ToListAsync();

                //model.TiposHardware = await _context.TipoHardwares
                //    .Where(t => t.Descripcion == "Hardware")
                //    .Select(t => new SelectListItem { Value = t.IdTipo.ToString(), Text = t.Descripcion })
                //    .ToListAsync();
                            
                return PartialView("_EditPartial", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error inesperado: " + ex.Message);

                // Recargar listas
                model.Marcas = await _context.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem { Value = m.IdMarca.ToString(), Text = m.Nombre })
                    .ToListAsync();

                model.TiposHardware = await _context.TipoHardwares
                    .Where(t => t.Descripcion == "Hardware")
                    .Select(t => new SelectListItem { Value = t.IdTipo.ToString(), Text = t.Descripcion })
                    .ToListAsync();
                
                return PartialView("_CreateEquipoPartial", model);
            }
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

            var Model = new DispositivoEditViewModel
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
                TempData["ErrorMessage"] = "No hay marcas o tipos disponibles.";
                return RedirectToAction("Index", "Dispositivo");
            }

            var tipoHardware = await _context.TipoHardwares
                               .FirstOrDefaultAsync(t => t.Descripcion == "Hardware");

            ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", Model.IdMarca);
            ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", Model.IdTipo);
            ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };
            ViewBag.IdTipoHardware = tipoHardware?.IdTipo;

            return PartialView("_EditPartial", Model);
        }

        // POST: Dispositivo/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DispositivoEditViewModel model)
        {
            // Validación temprana del ID
            if (model.IdDispositivo <= 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Recargar listas para el formulario
                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();
                var tipoHardware = await _context.TipoHardwares
                                   .FirstOrDefaultAsync(t => t.Descripcion == "Hardware");

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };
                ViewBag.IdTipoHardware = tipoHardware?.IdTipo;

                return PartialView("_EditPartial", model);
            }

            var dispositivo = await _context.Dispositivos.FindAsync(model.IdDispositivo);
            if (dispositivo == null)
            {
                return NotFound();
            }

            try
            {
                // Validar número de serie duplicado
                if (!string.IsNullOrWhiteSpace(model.NroSerie))
                {
                    var existeSerie = await _context.Dispositivos
                        .AnyAsync(c => c.NroSerie == model.NroSerie &&
                                       c.IdDispositivo != model.IdDispositivo &&
                                       c.EstadoRegistro);

                    if (existeSerie)
                    {
                        ModelState.AddModelError("NroSerie", "Ya existe otro Dispositivo con este número de serie.");

                        // Recargar datos para la vista
                        var marcasError = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                        var tiposError = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();
                        var tipoHardware = await _context.TipoHardwares
                                           .FirstOrDefaultAsync(t => t.Descripcion == "Hardware");

                        ViewData["IdMarca"] = new SelectList(marcasError, "IdMarca", "Nombre", model.IdMarca);
                        ViewData["IdTipo"] = new SelectList(tiposError, "IdTipo", "Descripcion", model.IdTipo);
                        ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };
                        ViewBag.IdTipoHardware = tipoHardware?.IdTipo;

                        return PartialView("_EditPartial", model);
                    }
                }

                // Actualizar campos del dispositivo
                dispositivo.Nombre = model.Nombre;
                dispositivo.Descripcion = model.Descripcion;
                dispositivo.IdMarca = model.IdMarca;
                dispositivo.IdTipo = model.IdTipo;
                dispositivo.NroSerie = model.NroSerie;
                dispositivo.Estado = model.Estado;
                dispositivo.FechaBaja = model.FechaBaja;
                dispositivo.StockActual = model.StockActual;
                dispositivo.StockMinimo = model.StockMinimo;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                var errorMessage = "Error al actualizar el Dispositivo.";

                if (ex.InnerException != null)
                {
                    var innerMessage = ex.InnerException.Message;

                    if (innerMessage.Contains("UQ__disposit__AD64A1611432F6A2") || innerMessage.Contains("nro_serie"))
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
                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();
                var tipoHardware = await _context.TipoHardwares
                                   .FirstOrDefaultAsync(t => t.Descripcion == "Hardware");

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };
                ViewBag.IdTipoHardware = tipoHardware?.IdTipo;

                return PartialView("_EditPartial", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error inesperado: " + ex.Message);

                // Recargar datos para la vista
                var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
                var tipos = await _context.TipoHardwares.OrderBy(t => t.Descripcion).ToListAsync();
                var tipoHardware = await _context.TipoHardwares
                                   .FirstOrDefaultAsync(t => t.Descripcion == "Hardware");

                ViewData["IdMarca"] = new SelectList(marcas, "IdMarca", "Nombre", model.IdMarca);
                ViewData["IdTipo"] = new SelectList(tipos, "IdTipo", "Descripcion", model.IdTipo);
                ViewBag.EstadosDisponibles = new List<string> { "Nuevo", "En uso", "Obsoleto" };
                ViewBag.IdTipoHardware = tipoHardware?.IdTipo;

                return PartialView("_EditPartial", model);
            }
        }



        // POST: Dispositivo/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dispositivo = await _context.Dispositivos.FindAsync(id);
            if (dispositivo != null)
            {
                dispositivo.EstadoRegistro = false;
                dispositivo.FechaBaja = DateTime.Now; // ✅ Registrar fecha y hora de baja
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        private bool DispositivoExists(int id)
        {
            return _context.Dispositivos.Any(e => e.IdDispositivo == id);
        }

        

    }
}