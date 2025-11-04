using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace inventario_coprotab.Controllers
{
    public class EquiposController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public EquiposController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // GET: Equipos
        public async Task<IActionResult> Index(string searchNombre, string searchSerie, string searchFecha)
        {
            var query = _context.Relacions
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdDispositivoNavigation)
                        .ThenInclude(d => d.IdTipoNavigation)
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdComponenteNavigation)
                        .ThenInclude(c => c.IdTipoNavigation)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchNombre))
            {
                query = query.Where(r => r.RelacionDetalles.Any(rd =>
                    rd.IdDispositivoNavigation != null &&
                    EF.Functions.Like(rd.IdDispositivoNavigation.Nombre.ToLower(), $"%{searchNombre.ToLower()}%")));
            }

            if (!string.IsNullOrEmpty(searchSerie))
            {
                query = query.Where(r => r.RelacionDetalles.Any(rd =>
                    rd.IdDispositivoNavigation != null &&
                    rd.IdDispositivoNavigation.NroSerie != null &&
                    rd.IdDispositivoNavigation.NroSerie.Contains(searchSerie)));
            }

            if (!string.IsNullOrEmpty(searchFecha))
            {
                if (DateTime.TryParse(searchFecha, out DateTime fecha))
                {
                    var fechaFiltro = DateOnly.FromDateTime(fecha);
                    query = query.Where(r => r.Fecha == fechaFiltro);
                }
            }

            var equipos = await query
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            ViewBag.SearchNombre = searchNombre;
            ViewBag.SearchSerie = searchSerie;
            ViewBag.SearchFecha = searchFecha;

            // Cargar la lista de responsables para el formulario de creación de equipos.
            // Se ordena por apellido y nombre para que sea más fácil de seleccionar.
            ViewBag.Responsables = await _context.Responsables
                .OrderBy(r => r.Apellido)
                .ThenBy(r => r.Nombre)
                .ToListAsync();

            return View(equipos);
        }

        // GET: Detalle del equipo
        [HttpGet]
        public async Task<IActionResult> DetalleEquipo(int id)
        {
            var equipo = await _context.Relacions
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdDispositivoNavigation)
                        .ThenInclude(d => d.IdTipoNavigation)
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdDispositivoNavigation)
                        .ThenInclude(d => d.IdMarcaNavigation)
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdComponenteNavigation)
                        .ThenInclude(c => c.IdTipoNavigation)
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdComponenteNavigation)
                        .ThenInclude(c => c.IdMarcaNavigation)
                .FirstOrDefaultAsync(r => r.IdRelacion == id);

            if (equipo == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsPartial", equipo);
        }

        // GET: Equipos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipo = await _context.Relacions
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdDispositivoNavigation)
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdComponenteNavigation)
                .FirstOrDefaultAsync(r => r.IdRelacion == id);

            if (equipo == null)
            {
                return NotFound();
            }

            var dispositivo = equipo.RelacionDetalles.FirstOrDefault()?.IdDispositivoNavigation;
            var componentesIds = equipo.RelacionDetalles
                .Select(rd => rd.IdComponente)
                .ToList();

            var model = new EquipoEditViewModel
            {
                IdRelacion = equipo.IdRelacion,
                IdDispositivo = dispositivo?.IdDispositivo ?? 0,
                DispositivoNombre = dispositivo?.Nombre ?? "N/A",
                ComponentesSeleccionados = componentesIds,
                ComponentesDisponibles = await _context.Componentes
                    .Where(c => c.EstadoRegistro)
                    .Include(c => c.IdMarcaNavigation)
                    .Select(c => new ComponenteCheckboxItem
                    {
                        IdComponente = c.IdComponente,
                        NombreCompleto = $"{c.Nombre} - {(c.NroSerie ?? "Sin serie")} ({(c.IdMarcaNavigation != null ? c.IdMarcaNavigation.Nombre : "Sin marca")})",
                        Seleccionado = componentesIds.Contains(c.IdComponente)
                    })
                    .ToListAsync()
            };

            return PartialView("_EditPartial", model);
        }

        // POST: Equipos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EquipoEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Eliminar detalles antiguos
                var detallesAntiguos = await _context.RelacionDetalles
                    .Where(rd => rd.IdRelacion == model.IdRelacion)
                    .ToListAsync();

                _context.RelacionDetalles.RemoveRange(detallesAntiguos);

                // Agregar nuevos detalles
                foreach (var componenteId in model.ComponentesSeleccionados)
                {
                    var nuevoDetalle = new inventario_coprotab.Models.DBInventario.RelacionDetalle
                    {
                        IdRelacion = model.IdRelacion,
                        IdDispositivo = model.IdDispositivo,
                        IdComponente = componenteId
                    };
                    _context.RelacionDetalles.Add(nuevoDetalle);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error al actualizar el equipo: " + ex.Message });
            }
        }

        // POST: Equipos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var relacion = await _context.Relacions
                    .Include(r => r.RelacionDetalles)
                    .FirstOrDefaultAsync(r => r.IdRelacion == id);

                if (relacion == null)
                {
                    return Json(new { success = false, message = "Equipo no encontrado" });
                }

                // Obtener IDs de dispositivo y componentes antes de eliminar
                var dispositivoId = relacion.RelacionDetalles.FirstOrDefault()?.IdDispositivo;
                var componentesIds = relacion.RelacionDetalles.Select(rd => rd.IdComponente).ToList();

                // Eliminar detalles
                _context.RelacionDetalles.RemoveRange(relacion.RelacionDetalles);

                // Eliminar relación
                _context.Relacions.Remove(relacion);

                await _context.SaveChangesAsync();

                // Reactivar dispositivo
                if (dispositivoId.HasValue)
                {
                    var dispositivo = await _context.Dispositivos.FindAsync(dispositivoId.Value);
                    if (dispositivo != null)
                    {
                        dispositivo.EstadoRegistro = true;
                    }
                }

                // Reactivar componentes
                var componentes = await _context.Componentes
                    .Where(c => componentesIds.Contains(c.IdComponente))
                    .ToListAsync();

                foreach (var componente in componentes)
                {
                    componente.EstadoRegistro = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error al eliminar el equipo: " + ex.Message });
            }
        }

        // GET: Buscar dispositivos
        [HttpGet]
        public async Task<IActionResult> BuscarDispositivos(string termino)
        {
            if (string.IsNullOrEmpty(termino) || termino.Length < 3)
            {
                return Json(new List<object>());
            }

            var dispositivos = await _context.Dispositivos
                .Include(d => d.IdTipoNavigation)
                .Include(d => d.IdMarcaNavigation)
                .Where(d => d.EstadoRegistro == true &&
                           d.IdTipoNavigation.Descripcion == "Hardware" &&
                           (d.Nombre.Contains(termino) ||
                            (d.NroSerie != null && d.NroSerie.Contains(termino))))
                .Select(d => new
                {
                    id = d.IdDispositivo,
                    nombre = d.Nombre,
                    nroSerie = d.NroSerie ?? "Sin serie",
                    marca = d.IdMarcaNavigation != null ? d.IdMarcaNavigation.Nombre : "Sin marca"
                })
                .Take(10)
                .ToListAsync();

            return Json(dispositivos);
        }

        // GET: Buscar componentes
        [HttpGet]
        public async Task<IActionResult> BuscarComponentes(string termino)
        {
            if (string.IsNullOrEmpty(termino) || termino.Length < 3)
            {
                return Json(new List<object>());
            }

            var componentes = await _context.Componentes
                .Include(c => c.IdTipoNavigation)
                .Include(c => c.IdMarcaNavigation)
                .Where(c => c.EstadoRegistro == true &&
                           (c.Nombre.Contains(termino) ||
                            (c.NroSerie != null && c.NroSerie.Contains(termino))))
                .Select(c => new
                {
                    id = c.IdComponente,
                    nombre = c.Nombre,
                    nroSerie = c.NroSerie ?? "Sin serie",
                    tipo = c.IdTipoNavigation != null ? c.IdTipoNavigation.Descripcion : "Sin tipo",
                    marca = c.IdMarcaNavigation != null ? c.IdMarcaNavigation.Nombre : "Sin marca"
                })
                .Take(10)
                .ToListAsync();

            return Json(componentes);
        }

        // POST: Crear equipo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEquipo([FromBody] CrearEquipoViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            if (modelo.Componentes == null || !modelo.Componentes.Any())
            {
                return Json(new { success = false, message = "Debe agregar al menos un componente" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var relacion = new inventario_coprotab.Models.DBInventario.Relacion
                {
                    Fecha = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Relacions.Add(relacion);
                await _context.SaveChangesAsync();

                foreach (var componenteId in modelo.Componentes)
                {
                    var detalle = new inventario_coprotab.Models.DBInventario.RelacionDetalle
                    {
                        IdRelacion = relacion.IdRelacion,
                        IdDispositivo = modelo.IdDispositivo,
                        IdComponente = componenteId
                    };

                    _context.RelacionDetalles.Add(detalle);

                    // Crear o actualizar la relación dispositivo/componente con responsable y observaciones.
                    var relacionExistente = await _context.RelacionDispositivoComponentes
                        .FirstOrDefaultAsync(rdc =>
                            rdc.IdDispositivo == modelo.IdDispositivo &&
                            rdc.IdComponente == componenteId);

                    if (relacionExistente == null)
                    {
                        relacionExistente = new inventario_coprotab.Models.DBInventario.RelacionDispositivoComponente
                        {
                            IdDispositivo = modelo.IdDispositivo,
                            IdComponente = componenteId
                        };
                        _context.RelacionDispositivoComponentes.Add(relacionExistente);
                    }

                    // Actualizar responsable y observaciones para este dispositivo/componente
                    relacionExistente.IdResponsable = modelo.IdResponsable;
                    relacionExistente.Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones)
                        ? null
                        : modelo.Observaciones;
                }

                await _context.SaveChangesAsync();

                // Actualizar estados
                var dispositivo = await _context.Dispositivos.FindAsync(modelo.IdDispositivo);
                if (dispositivo != null)
                {
                    dispositivo.EstadoRegistro = false;
                    await _context.SaveChangesAsync();
                }

                var componentes = await _context.Componentes
                    .Where(c => modelo.Componentes.Contains(c.IdComponente))
                    .ToListAsync();

                foreach (var componente in componentes)
                {
                    componente.EstadoRegistro = false;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Equipo creado exitosamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return Json(new
                {
                    success = false,
                    message = "Error al crear el equipo: " + ex.Message,
                    innerError = innerMessage
                });
            }
        }

    }

    public class CrearEquipoViewModel
    {
        public int IdDispositivo { get; set; }
        public List<int> Componentes { get; set; }

        // Identificador del responsable a cargo del equipo. Puede ser nulo cuando no se especifica un responsable.
        public int? IdResponsable { get; set; }

        // Observaciones generales sobre el equipo o la relación dispositivo/componente.
        public string? Observaciones { get; set; }
    }
}